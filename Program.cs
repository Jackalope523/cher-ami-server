using CherAmiAPI;
using CherAmiAPI.BackgroundJobs;
using CherAmiAPI.Contexts;
using CherAmiAPI.Endpoints.Circles;
using CherAmiAPI.Exceptions;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using CherAmiAPI.Shared.Mappers;
using CherAmiAPI.Shared.SharedMappers;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Quartz;
using QuestPDF.Infrastructure;
using Serilog;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using User = CherAmiAPI.Entities.User;

QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.UseEnvironmentFonts = false;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web", Version = "v1" });
});

Log.Logger = new LoggerConfiguration()
          .MinimumLevel.Error()
          .WriteTo.Console()
          .WriteTo.AzureApp()
          .CreateLogger();

builder.Configuration.AddAzureKeyVault(
    new Uri(builder.Configuration["KEY_VAULT_URI"]),
    new DefaultAzureCredential());

if (builder.Environment.IsProduction())
{
    builder.Services.AddDbContext<ApplicationDbContext, AzureSQLProductionContext>();
}
else if (builder.Environment.IsStaging())
{
    builder.Services.AddDbContext<ApplicationDbContext, AzureSQLStagingContext>();
}
else if (builder.Environment.IsDevelopment())
{
    throw new UnknownEnvironmentException("Development environment is not supported. Use staging or production on Azure.");
}
else
{
    throw new UnknownEnvironmentException($"Unrecognized environment: {builder.Environment.EnvironmentName}");
}

builder.Services.AddSingleton<ImageUploadCoordinator>();
builder.Services.AddScoped<IKeyService, KeyService>();
builder.Services.AddScoped<IImageService, AzureImageService>();
builder.Services.AddScoped<IInviteCodeService, InviteCodeService>();
builder.Services.AddScoped<INameService, NameService>();
builder.Services.AddScoped<IPhotoDateService, PhotoDateService>();
builder.Services.AddScoped<CircleService>();

builder.Services.AddScoped<UserItemMapper>();
builder.Services.AddScoped<RecipientItemMapper>();
builder.Services.AddScoped<FeedPostMapper>();

builder.Services.AddHttpClient();

builder.Services.AddHttpClient<OneSignalService>(client =>
{
    client.BaseAddress = new Uri($"https://api.onesignal.com/apps/{builder.Configuration["ONESIGNAL_APP_ID"]}/");
    client.DefaultRequestHeaders.Add("Authorization", $"key {builder.Configuration["OneSignal-API-Key"]}");
});

StripeConfiguration.ApiKey = builder.Configuration["Stripe-Secret-Key"];
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<SubscriptionItemService>();
builder.Services.AddScoped<SetupIntentService>();
builder.Services.AddScoped<CustomerPaymentMethodService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<PriceService>();
builder.Services.AddScoped<PaymentMethodService>();

builder.Services
    .AddIdentityCore<User>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthenticationJwtBearer(s => s.SigningKey = builder.Configuration["Cher-Ami-API-Signing-Key"])
    .AddAuthorization()
    .AddFastEndpoints();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Website", policy =>
    {
        policy.WithOrigins("https://www.thecherami.com", "https://thecherami.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddQuartz(options =>
{
    //JobKey publishMagazineJobKey = JobKey.Create(nameof(PublishMagazinesJob));
    //options.AddJob<PublishMagazinesJob>(publishMagazineJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(publishMagazineJobKey).StartNow());
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web v1"));
}

app.UseRouting();

app.UseCors("Website");

app.UseExceptionHandler();

app.UseAuthentication()
   .UseAuthorization()
   .UseFastEndpoints();

app.Run();
