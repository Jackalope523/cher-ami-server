using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using CherAmiAPI;
using CherAmiAPI.Contexts;
using CherAmiAPI.Endpoints.Circles;
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
using User = CherAmiAPI.Entities.User;
using CherAmiAPI.BackgroundJobs;
using System.Net.Http;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

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

Log.Logger = new LoggerConfiguration()
          .MinimumLevel.Error()
          .WriteTo.Console()
          .WriteTo.AzureApp()
          .CreateLogger();

// JACKALOPE: Set up conflict. 
builder.Services.AddDbContext<ApplicationDbContext, AzureSQLStagingContext>();

KeyService keyService = new();

builder.Services.AddScoped<IKeyService, KeyService>();
builder.Services.AddScoped<IImageService, AzureImageService>();
builder.Services.AddScoped<IInviteCodeService, inviteCodeService>();

builder.Services.AddScoped<UserItemMapper>();
builder.Services.AddScoped<RecipientItemMapper>();
builder.Services.AddScoped<FeedPostMapper>();

builder.Services.AddScoped<HttpClient>();

// JACKALOPE: Key vault man,
StripeConfiguration.ApiKey = await keyService.GetSecretAsync("Stripe-Secret-Key");
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<SubscriptionItemService>();
builder.Services.AddScoped<SetupIntentService>();
builder.Services.AddScoped<CustomerPaymentMethodService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<PriceService>();

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
    //JobKey publishMagazineJobKey = JobKey.Create(nameof(PublishMagazinesJob));
    //options.AddJob<PublishMagazinesJob>(publishMagazineJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(publishMagazineJobKey).WithCronSchedule("0 0 0 1 * ?"));

    //JobKey updateSubscriptionsJobKey = JobKey.Create(nameof(UpdateSubscriptionsJob));
    //options.AddJob<UpdateSubscriptionsJob>(updateSubscriptionsJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(updateSubscriptionsJobKey).WithCronSchedule("0 */1 * * * ?"));

    //JobKey monthlyIssueJobKey = JobKey.Create(nameof(MonthlyIssueJob));
    //options.AddJob<MonthlyIssueJob>(monthlyIssueJobKey);
    //options.AddTrigger(trigger => trigger.ForJob(monthlyIssueJobKey).WithCronSchedule("0 * * * * ?"));
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

app.UseCors("_HollowSpecificOrigins");

app.UseExceptionHandler();

app.UseAuthentication()
   .UseAuthorization()
   .UseFastEndpoints();

app.Run();
