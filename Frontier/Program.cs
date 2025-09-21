using CrazyLizard;
using CrazyLizard.Boundaries.Repository;
using CrazyLizard.Boundaries.Service;
using CrazyLizard.Contexts;
using CrazyLizard.Entities;
using CrazyLizard.Exceptions;
using CrazyLizard.Interfaces;
using CrazyLizard.Interfaces.Repository;
using CrazyLizard.Interfaces.Service;
using CrazyLizard.Repositories;
using CrazyLizard.Services;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using Stripe;
using System.IO;
using AccountService = CrazyLizard.Services.AccountService;

var builder = WebApplication.CreateBuilder(args);

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
    throw new UnknownEnvironmentException("Unknown ASPNETCORE_ENVIRONMENT set.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("_HollowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://almostcanary.com");
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web", Version = "v1" });
});

var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);

var frontierLogger = loggerFactory.CreateLogger("Frontier");
var coreLogger = loggerFactory.CreateLogger("Core");


//var keyProvider = new KeyStorageRepository(new LLContext());

//OneSignalService oneSignalInstance = new();
//OneSignalService.Initialise(frontierLogger,
//    keyProvider.GetHollowOneSignalApiKeyAsync().Result,
//    keyProvider.GetHollowOneSignalAppIdAsync().Result);

//TwilioService.Initialise(env, frontierLogger,
//    keyProvider.GetHollowTwilioAccountKeyAsync().Result,
//    keyProvider.GetHollowTwilioAuthTokenAsync().Result,
//    keyProvider.GetHollowTwilioMessagingServiceAsync().Result);


OneSignalService oneSignalInstance = new();
OneSignalService.Initialise(frontierLogger,
    "",
    "");

TwilioService.Initialise(env, frontierLogger,
    "",
    "",
    "");

builder.Services.AddTransient<INotificationService, OneSignalService>(_ => oneSignalInstance);
builder.Services.AddScoped<IEmailService, OneSignalService>();
builder.Services.AddTransient<ISMSService, TwilioService>();

string prodString = "Server=tcp:sparrow-stores.database.windows.net,1433;Initial Catalog=CanaryProduction;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";

//builder.Services.AddDbContext<LLContext>(options =>
//   options.
//   UseSqlServer
//   (
//       prodString,
//       x => x.
//       MigrationsHistoryTable("__ProductionMigrationsHistory").
//       EnableRetryOnFailure()
//   )
//);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=dev.db"));

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICircleRepository, CircleRepository>();
builder.Services.AddScoped<IIssueRepository, IssueRepository>();
builder.Services.AddScoped<IKeyRepository, KeyStorageRepository>();


string prodUri = "https://{0}.blob.core.windows.net/canaryproduction";

builder.Services.AddScoped<IMediaRepository>(provider =>
{
    var dbContext = provider.GetRequiredService<ApplicationDbContext>();
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

builder.Services.AddScoped<StripeClient>(_ => new("sk_test_51RxlM1ARYKi6NXMeFaJIdN2b1vx6HARAG3uqvYlYcPoqvexFzll5R1fXXtPq7HVBuA4DYJEjjFkG1pSJ76UgNEoM00rz3BvxnY"));

builder.Services
    .AddIdentityCore<User>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/home/data-protection-keys"))
    .SetApplicationName($"cardinal-{env}-keys");

builder.Services
    .AddAuthenticationJwtBearer(s => s.SigningKey = "b10fa28c-9390-45a1-88b7-dff66ae71e0c")
    .AddAuthorization()
    .AddFastEndpoints();

builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web v1"));
}

app.UseRouting();

app.UseCors("_HollowSpecificOrigins");

app.UseExceptionHandler();

app.UseAuthentication()
   .UseAuthorization()
   .UseFastEndpoints();

app.Run();
