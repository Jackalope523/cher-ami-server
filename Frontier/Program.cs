using Core;
using Core.Boundaries;
using Frontier;
using Frontier.Controllers;
using Frontier.Services;
using Frontier.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Repository;
using Serilog;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.AzureApp()
    .MinimumLevel.Debug()
    .CreateLogger();

builder.Host.UseSerilog();

var configuration = builder.Configuration;

string env = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
var flag = env switch
{
    "Production" => EnvironmentFlag.Production,
    "Staging" => EnvironmentFlag.Staging,
    "Development" => EnvironmentFlag.Development,
    _ => throw new InvalidEnvironmentException("Unknown ASPNETCORE_ENVIRONMENT set.")
};

var environment = new EnvironmentOptions { Flag = flag };

builder.Services.AddCors(options =>
{
    options.AddPolicy("_HollowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://almostcanary.com");
    });
});

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web", Version = "v1" });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);

var frontierLogger = loggerFactory.CreateLogger("Frontier");
var coreLogger = loggerFactory.CreateLogger("Core");
var repositoryLogger = loggerFactory.CreateLogger("Repository");

Harbor harbor = environment.IsProduction ?
    new Harbor(Harbor.Flag.Production, repositoryLogger) :
    (environment.Flag == EnvironmentFlag.Staging ?
        new Harbor(Harbor.Flag.Staging, repositoryLogger) :
        new Harbor(Harbor.Flag.Development, repositoryLogger));

var keyProvider = harbor.KeyDatabaseAccess;

OneSignalService oneSignalInstance = new();
OneSignalService.Initialise(frontierLogger,
    keyProvider.GetHollowOneSignalApiKeyAsync().Result,
    keyProvider.GetHollowOneSignalAppIdAsync().Result);

TwilioService.Initialise(environment, frontierLogger,
    keyProvider.GetHollowTwilioAccountKeyAsync().Result,
    keyProvider.GetHollowTwilioAuthTokenAsync().Result,
    keyProvider.GetHollowTwilioMessagingServiceAsync().Result);

builder.Services.AddTransient<INotificationService, OneSignalService>(_ => oneSignalInstance);
builder.Services.AddTransient<ISMSService, TwilioService>();

CoreTerminal terminal = CoreTerminal.CreateTerminal(
    environment,
    coreLogger,
    harbor.AccountDatabaseAccess,
    harbor.CircleDatabaseAccess,
    harbor.IssueDatabaseAccess,
    harbor.ReportDatabaseAccess,
    harbor.KeyDatabaseAccess,
    harbor.MediaDatabaseAccess,
    harbor.NotificationDatabaseAccess,
    harbor.OrderDatabaseAccess,
    harbor.ProfileDatabaseAccess,
    harbor.MiscellaneousDatabaseAccess,
    oneSignalInstance
);

ControllerBox box = new(
    environment,
    frontierLogger,
    terminal.AccountOperations,
    terminal.ProfileOperations,
    terminal.CircleOperations,
    terminal.IssueOperations,
    terminal.KeyOperations,
    terminal.ReportOperations,
    terminal.MediaOperations,
    terminal.NotificationOperations,
    terminal.OrderOperations,
    terminal.MiscellaneousOperations
);

builder.Services.AddSingleton(box);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddIdentityCookies();

builder.Services.AddIdentityCore<CoreUser>()
    .AddUserStore<UserAccountStore>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/home/data-protection-keys"))
    .SetApplicationName($"cardinal-{env}-keys");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web v1"));
}

app.UseExceptionHandler("/error");

app.UseRouting();

app.UseCors("_HollowSpecificOrigins");

app.UseAuthentication();
app.UseCookiePolicy();
app.UseAuthorization();

app.MapControllers();

app.Run();
