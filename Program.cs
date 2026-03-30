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

KeyService keyService = new(builder.Configuration);

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
builder.Services.AddScoped<CircleService>();

builder.Services.AddScoped<UserItemMapper>();
builder.Services.AddScoped<RecipientItemMapper>();
builder.Services.AddScoped<FeedPostMapper>();

builder.Services.AddHttpClient();

string oneSignalApiKey = await keyService.GetSecretAsync("OneSignal-API-Key");
builder.Services.AddHttpClient<OneSignalService>(client =>
{
    client.BaseAddress = new Uri($"https://api.onesignal.com/apps/{builder.Configuration["ONESIGNAL_APP_ID"]}/");
    client.DefaultRequestHeaders.Add("Authorization", $"key {oneSignalApiKey}");
});

StripeConfiguration.ApiKey = await keyService.GetSecretAsync("Stripe-Secret-Key");
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

string signingKey = await keyService.GetSecretAsync("Cher-Ami-API-Signing-Key");

builder.Services
    .AddAuthenticationJwtBearer(s => s.SigningKey = signingKey)
    .AddAuthorization()
    .AddFastEndpoints();

builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddQuartz(options =>
{
    //JobKey syncTagsJobKey = JobKey.Create(nameof(SyncOneSignalTagsJob));
    //options.AddJob<SyncOneSignalTagsJob>(syncTagsJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(syncTagsJobKey).StartNow());

    //JobKey removeProspectiveTagsJobKey = JobKey.Create(nameof(RemoveProspectiveJoinedAtTagsJob));
    //options.AddJob<RemoveProspectiveJoinedAtTagsJob>(removeProspectiveTagsJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(removeProspectiveTagsJobKey).StartNow());

    //JobKey addEmailSubscriptionJobKey = JobKey.Create(nameof(AddEmailSubscriptionJob));
    //options.AddJob<AddEmailSubscriptionJob>(addEmailSubscriptionJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(addEmailSubscriptionJobKey).StartNow());

    //JobKey removeJoinedAtTagsJobKey = JobKey.Create(nameof(RemoveJoinedAtTagsJob));
    //options.AddJob<RemoveJoinedAtTagsJob>(removeJoinedAtTagsJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(removeJoinedAtTagsJobKey).StartNow());

    //JobKey updateJoinDateJobKey = JobKey.Create(nameof(UpdateJoinDateJob));
    //options.AddJob<UpdateJoinDateJob>(updateJoinDateJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(updateJoinDateJobKey).StartNow());

    //JobKey publishMagazineJobKey = JobKey.Create(nameof(PublishMagazinesJob));
    //options.AddJob<PublishMagazinesJob>(publishMagazineJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(publishMagazineJobKey).WithCronSchedule("0 0 6 1 * ?"));

    //JobKey publishMagazineJobKey = JobKey.Create(nameof(PublishMagazinesJob));
    //options.AddJob<PublishMagazinesJob>(publishMagazineJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(publishMagazineJobKey).StartNow());

    //JobKey updateSubscriptionsJobKey = JobKey.Create(nameof(UpdateSubscriptionsJob));
    //options.AddJob<UpdateSubscriptionsJob>(updateSubscriptionsJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(updateSubscriptionsJobKey).WithCronSchedule("0 0 8 1 * ?"));

    //JobKey keepAliveJobKey = JobKey.Create(nameof(KeepAliveJob));
    //options.AddJob<KeepAliveJob>(keepAliveJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(keepAliveJobKey).WithCronSchedule("0 0/15 13-23 * * ?"));
    //options.AddTrigger(trigger => trigger.ForJob(keepAliveJobKey).WithCronSchedule("0 0/15 0-2 * * ?"));

    //JobKey fixJobKey = JobKey.Create(nameof(FixJob));
    //options.AddJob<FixJob>(fixJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(fixJobKey).StartNow());

    //JobKey importProspectiveUsersJobKey = JobKey.Create(nameof(ImportProspectiveUsersJob));
    //options.AddJob<ImportProspectiveUsersJob>(importProspectiveUsersJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(importProspectiveUsersJobKey).StartNow());

    //JobKey addMarketingTagsJobKey = JobKey.Create(nameof(AddMarketingTagsJob));
    //options.AddJob<AddMarketingTagsJob>(addMarketingTagsJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(addMarketingTagsJobKey).StartNow());
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

app.UseExceptionHandler();

app.UseAuthentication()
   .UseAuthorization()
   .UseFastEndpoints();

app.Run();
