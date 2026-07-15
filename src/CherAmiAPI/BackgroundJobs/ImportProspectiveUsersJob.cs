using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using CherAmiAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CherAmiAPI.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class ImportProspectiveUsersJob(IServiceProvider _serviceProvider) : IJob
    {
        private record UserImportData(string Email, Guid ExternalId, string OneSignalId);

        private readonly List<UserImportData> _usersToImport = 
        [
            // Template: new UserImportData("email@example.com", Guid.Parse("00000000-0000-0000-0000-000000000000"), "onesignal-id-here"),
            new UserImportData("abihuffman20@gmail.com", Guid.Parse("c7458967-d5ac-4e6b-8249-f1da6280a044"), "79d1ce23-5727-45d1-890f-147e63ce2486"),
            new UserImportData("mckozey@sbcglobal.net", Guid.Parse("b6e58cfd-ec03-4c55-9323-9f026603974c"), "3bc9cdd5-8d38-410f-9e71-f772a83f6645"),
            new UserImportData("williegail53@yahoo.com", Guid.Parse("2890ad79-81bd-43f8-84c1-4c1efc1e4e40"), "30cb8684-bf95-4638-a573-1b8219e60ab5"),
            new UserImportData("cialynnmk@gmail.com", Guid.Parse("f0706ec0-13d4-461b-b6bd-2584db1dafb7"), "90882f3f-1809-4564-b8d9-7a62dafdbae4"),
            new UserImportData("dixonlisa309@gmail.com", Guid.Parse("a1dfd298-be8f-4c50-a92d-c84356dea4e9"), "d025c2be-9469-47ae-8f5d-a60c8ccfd9f1"),
            new UserImportData("amy.lynn.jones66@gmail.com", Guid.Parse("a9063a0f-6f90-41cd-a257-5d45f7618d25"), "20567119-e37d-4521-a1be-368e58e0aa07"),
            new UserImportData("lisaconnell512@gmail.com", Guid.Parse("5f05119b-98e6-4be9-a1c6-a8bfc7383279"), "7ec6786d-7088-4ae1-83cc-3e86d5ec0520"),
            new UserImportData("gibsonloretta23@gmail.com", Guid.Parse("ac8115b4-0cd7-46fe-8f77-a432411aef82"), "9d2f4a56-a4ea-42cc-92d7-35dea69a7a18"),
        ];

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            OneSignalService oneSignalService = scope.ServiceProvider.GetRequiredService<OneSignalService>();

            Log.Error("Starting ImportProspectiveUsersJob: Importing {Count} users.", _usersToImport.Count);

            int successCount = 0;
            int failureCount = 0;
            int skippedCount = 0;

            foreach (var importData in _usersToImport)
            {
                try
                {
                    var existingUser = await ctx.Users
                        .AnyAsync(u => u.Email == importData.Email);

                    if (existingUser)
                    {
                        Log.Error("User {Email} already exists. Skipping.", importData.Email);
                        skippedCount++;
                        continue;
                    }

                    User newUser = new()
                    {
                        UserName = importData.Email,
                        Email = importData.Email,
                        ExternalId = importData.ExternalId,
                        OneSignalId = importData.OneSignalId,
                        AccountStatus = UserAccountStatus.Prospective,
                    };

                    // Add tags to existing OneSignal user
                    try 
                    {
                        await oneSignalService.AddTagAsync(newUser.ExternalId, "email_reminders", "1");
                        await oneSignalService.AddTagAsync(newUser.ExternalId, "email_marketing", "1");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to add tags for user {Email}, but continuing with creation.", importData.Email);
                    }

                    var result = await userManager.CreateAsync(newUser);

                    if (result.Succeeded)
                    {
                        successCount++;
                        Log.Error("Successfully imported user: {Email}", importData.Email);
                    }
                    else
                    {
                        failureCount++;
                        foreach (var error in result.Errors)
                        {
                            Log.Error("Error creating user {Email}: {Error}", importData.Email, error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to import user {Email}", importData.Email);
                    failureCount++;
                }
            }

            Log.Error("ImportProspectiveUsersJob completed. Success: {SuccessCount}, Failure: {FailureCount}, Skipped: {SkippedCount}", 
                successCount, failureCount, skippedCount);
        }
    }
}
