using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CherAmiAPI.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class PublishMagazinesJob(IServiceProvider _serviceProvider) : IJob
    {
        private readonly TokenCredential _credentials = new DefaultAzureCredential();
        private readonly string _storageAccountUri = "";

        private async Task UploadBlobAsync(string path, MemoryStream blob)
        {
            string[] parts = path.Split(new[] { '/' }, 2);
            var (containerName, blobName) = (parts[0], parts[1]);

            BlobContainerClient containerClient = new(new Uri($"{_storageAccountUri}/{containerName}"), _credentials);

            await containerClient.CreateIfNotExistsAsync();
            blob.Position = 0;

            await containerClient.GetBlobClient(blobName).UploadAsync(blob, overwrite: true);
        }

        private async Task<MemoryStream> DownloadBlobAsync(string path)
        {
            MemoryStream stream = new();
            BlobClient blobClient = new(new Uri($"{_storageAccountUri}/{path}"), _credentials);

            await blobClient.DownloadToAsync(stream);
            stream.Position = 0;
            return stream;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            List<Issue> issues = await ctx.Issues.Where(x => x.Status == IssueStatus.Drafting).Include(x => x.Posts).ToListAsync();

            foreach (Issue issue in issues)
            {
                using MemoryStream memoryStream = new();

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(20));

                        page.Header()
                            .Text("Hello PDF!")
                            .SemiBold().FontSize(36).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(x =>
                            {
                                x.Spacing(20);

                                x.Item().Text(Placeholders.LoremIpsum());
                                x.Item().Image(Placeholders.Image(200, 100));
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                            });
                    });
                }).GeneratePdf(memoryStream);

                await UploadBlobAsync($"circles/{issue.CircleId}/issues/{issue.Id}/magazine.pdf", memoryStream);
                issue.Status = IssueStatus.Published;

                Issue toAdd = new()
                {
                    CircleId = issue.CircleId,
                    Title = $"{DateTime.UtcNow:MMMM yyyy} · Issue {issue.IssueNumber + 1}",
                    IssueNumber = issue.IssueNumber + 1,
                    DraftingStart = new DateTimeOffset(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1), TimeSpan.Zero),
                    DraftingEnd = new DateTimeOffset(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).AddTicks(-1), TimeSpan.Zero),
                    Status = IssueStatus.Drafting,
                };

                ctx.Issues.Add(toAdd);
            }

            await ctx.SaveChangesAsync();
        }
    }
}
