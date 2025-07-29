using System;
using Core.Boundaries;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Frontier.Stores;
using Frontier.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Repository;
using Frontier.Controllers;
using Microsoft.Extensions.Logging;
using Core;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

namespace Frontier
{
    public class Ignition
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.AzureApp()
                .MinimumLevel.Debug()
                .CreateLogger();

            Log.Information("Hollow starting up...");

            try
            {
                var app = CreateHostBuilder(args)
                    .UseSerilog()
                    .Build();

                // Inject socket

                var loggerFactory = new LoggerFactory()
                    .AddSerilog(Log.Logger);

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Hollow failed unexpectedly.");
            }
            finally
            {
                Log.Debug("Hollow shutting down.");
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Ignition>();
                });

        public static string HollowSpecificOrigins = "_HollowSpecificOrigins";

        public IConfiguration Configuration { get; }

        public Ignition(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string env = Configuration["ASPNETCORE_ENVIRONMENT"];

            var flag = env switch
            {
                "Production" => EnvironmentFlag.Production,
                "Staging" => EnvironmentFlag.Staging,
                "Development" => EnvironmentFlag.Development,
                _ => throw new InvalidEnvironmentException("Unknown ASPNETCORE_ENVIRONMENT set.")
            };

            EnvironmentOptions environment = new() { Flag = flag };

            services.AddCors(options =>
            {
                options.AddPolicy(name: HollowSpecificOrigins,
                    policy =>
                    {
                        policy.WithOrigins("https://almostcanary.com");
                    });
            });

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web", Version = "v1" });
            });

            /////
            // Loggers
            ////////////

            var loggerFactory = new LoggerFactory()
                .AddSerilog(Log.Logger);

            var frontierLogger = loggerFactory.CreateLogger("Frontier");
            var coreLogger = loggerFactory.CreateLogger("Core");
            var repositoryLogger = loggerFactory.CreateLogger("Repository");
            
            /////
            // Database
            /////////////

            Harbor harbor;

            if (environment.IsProduction)
            {
                harbor = new(Harbor.Flag.Production, repositoryLogger);
            }
            else if (environment.Flag.Equals(EnvironmentFlag.Staging))
            {
                harbor = new(Harbor.Flag.Staging, repositoryLogger);
            }
            else
            {
                harbor = new(Harbor.Flag.Development, repositoryLogger);
            }

            var keyProvider = harbor.KeyDatabaseAccess;

            /////
            // Services 
            /////////////

            OneSignalService oneSignalInstance = new();
            OneSignalService.Initialise(frontierLogger,
                keyProvider.GetHollowOneSignalApiKeyAsync().Result,
                keyProvider.GetHollowOneSignalAppIdAsync().Result);
            
            TwilioService.Initialise(environment, frontierLogger,
                keyProvider.GetHollowTwilioAccountKeyAsync().Result,
                keyProvider.GetHollowTwilioAuthTokenAsync().Result,
                keyProvider.GetHollowTwilioMessagingServiceAsync().Result);

            services.AddTransient<INotificationService, OneSignalService>(service => oneSignalInstance);
            services.AddTransient<ISMSService, TwilioService>();

            //////
            // Connections
            ////////////////

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

            services.AddSingleton(box);

            /////
            // Daemons
            ////////////

            // None atm

            /////////
            // Authentication Schema 
            //////////////////////////

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            })
                .AddIdentityCookies();

            services.AddIdentityCore<CoreUser>()
                .AddUserStore<UserAccountStore>()
                .AddSignInManager()
                .AddDefaultTokenProviders();
            
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"/home/data-protection-keys"))
                .SetApplicationName($"cardinal-{env}-keys");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web v1"));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(HollowSpecificOrigins);

            app.UseAuthentication();
            app.UseCookiePolicy();
            app.UseAuthorization();

            app.UseExceptionHandler("/error");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public static bool IsDebug
        {
            get
            {
                bool isDebug = false;
#if DEBUG
				isDebug = true;
#endif
                return isDebug;
            }
        }
    }
}