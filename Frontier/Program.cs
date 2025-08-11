using Frontier;
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
using FastEndpoints;
using Repository.Repositories;
using Microsoft.EntityFrameworkCore;
using Repository.Contexts;
using Microsoft.Extensions.Configuration;
using LazyLizardBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();

Log.Logger = new LoggerConfiguration()
    .WriteTo.AzureApp()
    .MinimumLevel.Debug()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddSingleton(Log.Logger);

var configuration = builder.Configuration;

string env = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";

if (env != "Production" && env != "Staging" && env != "Development")
{
    throw new InvalidEnvironmentException("Unknown ASPNETCORE_ENVIRONMENT set.");
}

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

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);

var frontierLogger = loggerFactory.CreateLogger("Frontier");
var coreLogger = loggerFactory.CreateLogger("Core");


var keyProvider = new KeyStorageRepository(new LLContext());

OneSignalService oneSignalInstance = new();
OneSignalService.Initialise(frontierLogger,
    keyProvider.GetHollowOneSignalApiKeyAsync().Result,
    keyProvider.GetHollowOneSignalAppIdAsync().Result);

TwilioService.Initialise(env, frontierLogger,
    keyProvider.GetHollowTwilioAccountKeyAsync().Result,
    keyProvider.GetHollowTwilioAuthTokenAsync().Result,
    keyProvider.GetHollowTwilioMessagingServiceAsync().Result);

builder.Services.AddTransient<INotificationService, OneSignalService>(_ => oneSignalInstance);
builder.Services.AddTransient<ISMSService, TwilioService>();

string prodString = "Server=tcp:sparrow-stores.database.windows.net,1433;Initial Catalog=CanaryProduction;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";

builder.Services.AddDbContext<LLContext>(options =>
   options.
   UseSqlServer
   (
       prodString,
       x => x.
       MigrationsHistoryTable("__ProductionMigrationsHistory").
       EnableRetryOnFailure()
   )
);

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICircleRepository, CircleRepository>();
builder.Services.AddScoped<IIssueRepository, IssueRepository>();
builder.Services.AddScoped<IKeyRepository, KeyStorageRepository>();


string prodUri = "https://{0}.blob.core.windows.net/canaryproduction";

builder.Services.AddScoped<IMediaRepository>(provider =>
{
    var dbContext = provider.GetRequiredService<LLContext>();
    return new MediaRepository(prodUri, dbContext);
});


builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IMiscellaneousRepository, MiscellaneousRepository>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICircleService, CircleService>();
builder.Services.AddScoped<IIssueService, IssueService>();
builder.Services.AddScoped<IKeyService, KeyService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<INotificationStorageService, NotificationStorageService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IMiscellaneousService, MiscellaneousService>();


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

app.UseExceptionHandler();

app.UseRouting();

app.UseCors("_HollowSpecificOrigins");

app.UseAuthentication();
app.UseCookiePolicy();
app.UseAuthorization();

app.UseFastEndpoints();

app.MapControllers();

app.Run();
