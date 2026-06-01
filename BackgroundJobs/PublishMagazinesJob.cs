using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using CherAmiAPI.Components;
using CherAmiAPI.Components.Layouts;
using CherAmiAPI.Components.Pages;
using CherAmiAPI.Contexts;
using CherAmiAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Serilog;
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
        private readonly string _storageAccountUri = "https://stcheramidataprod.blob.core.windows.net";
        string charcoal800 = "#242832";
        string orange = "#C15F3C";
        string white = "#FFFFFF";

        private async Task UploadBlobAsync(string path, MemoryStream blob)
        {
            string[] parts = path.Split(['/'], 2);
            var (containerName, blobName) = (parts[0], parts[1]);

            BlobContainerClient containerClient = new(new Uri($"{_storageAccountUri}/{containerName}"), _credentials);

            await containerClient.CreateIfNotExistsAsync();
            blob.Position = 0;

            await containerClient.GetBlobClient(blobName).UploadAsync(blob, overwrite: true);
        }

        private async Task<byte[]> DownloadBlobAsync(string path)
        {
            using MemoryStream stream = new();
            BlobClient blobClient = new(new Uri($"{_storageAccountUri}/{path}"), _credentials);

            await blobClient.DownloadToAsync(stream);
            return stream.ToArray();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


            Log.Error("Getting issues...");
            var issues = await ctx.Issues
                                  .Where(x => x.Status == IssueStatus.Drafting && x.DraftingEnd < DateTimeOffset.UtcNow)
                                  .Select(x => new { Issue = x, CircleId = x.Circle.Id, CircleTitle = x.Circle.Title })
                                  .ToListAsync();


            foreach (var issue in issues)
            {
                Log.Error($"Publishing issue {issue.Issue.Id}.");

                List<Recipient> recipients = await ctx.Recipients
                                               .Where(x => x.Manager.CircleId == issue.CircleId)
                                               .ToListAsync();

                Log.Error($"Getting posts from database...");
                var posts = await ctx.Posts
                                      .Where(x => x.IssueId == issue.Issue.Id)
                                      .Select(x => new { x.LowResolutionImagePath, x.ImageWidth, x.ImageHeight, x.Author.AvatarPath, AuthorName = $"{x.Author.FirstName} {x.Author.LastName}", Text = x.Caption })
                                      .ToListAsync();

                if (recipients.Count == 0 || posts.Count == 0)
                {
                    Log.Error($"Marking issue {issue.Issue.Id} as unreleased.");
                    issue.Issue.Status = posts.Count == 0 ? IssueStatus.Empty : IssueStatus.Unreleased;

                    Issue toAddAnyway = new()
                    {
                        CircleId = issue.CircleId,
                        Title = $"{DateTime.UtcNow:MMMM yyyy} · Issue {issue.Issue.IssueNumber + 1}",
                        IssueNumber = issue.Issue.IssueNumber + 1,
                        DraftingStart = new DateTimeOffset(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1), TimeSpan.Zero),
                        DraftingEnd = new DateTimeOffset(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).AddTicks(-1), TimeSpan.Zero),
                        Status = IssueStatus.Drafting,
                    };

                    ctx.Issues.Add(toAddAnyway);
                    continue;
                }

                Log.Error($"Mapping posts to props...");
                List<PostComponentProps> postComponentProps = [];
                foreach (var post in posts)
                {
                    Log.Error($"Mapping post by {post.AuthorName}");

                    PostComponentProps props = new()
                    {
                        Image = await DownloadBlobAsync(post.LowResolutionImagePath),
                        ImageHeight = post.ImageHeight,
                        ImageWidth = post.ImageWidth,
                        AuthorAvatar = post.AvatarPath != null ? await DownloadBlobAsync(post.AvatarPath) : null,
                        AuthorName = post.AuthorName,
                        Text = post.Text
                    };

                    postComponentProps.Add(props);
                }

                List<(Template template, List<PostComponentProps> posts)> templatePostPairs = MagazinePlanner.Plan(postComponentProps);

                Log.Error($"Making PDFs...");
                foreach (Recipient recipient in recipients) 
                {
                    Log.Error($"Making pdf for {recipient.Name}.");
                    MemoryStream memoryStream = new();

                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.Letter);
                            page.Margin(0.5f, Unit.Inch);
                            page.PageColor(white);

                            page.Header()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                       .Text(issue.Issue.Title)
                                       .FontFamily("Poppins")
                                       .FontSize(18)
                                       .FontColor(orange)
                                       .Medium();

                                    row.RelativeItem()
                                       .Text(issue.CircleTitle)
                                       .FontFamily("Damion")
                                       .FontSize(18)
                                       .FontColor(orange)
                                       .Medium()
                                       .AlignRight();
                                });

                            page.Content()
                                .Column(column =>
                                {
                                    column.Spacing(0.5f, Unit.Inch);

                                    column.Item()
                                          .PaddingTop(0.5f, Unit.Inch)
                                          .Height(1.6776f, Unit.Inch)
                                          .Image("Assets\\Images\\logo.png")
                                          .FitArea();

                                    column.Item()
                                          .AlignCenter()
                                          .PaddingTop(0.5f, Unit.Inch)
                                          .Height(4.9797f, Unit.Inch)
                                          .Image("Assets\\Images\\hedgehog.png")
                                          .FitArea();
                                });

                            page.Footer()
                                .Row(row =>
                                {
                                    row.Spacing(0.25f, Unit.Inch);

                                    row.RelativeItem()
                                       .AlignMiddle()
                                       .LineHorizontal(3)
                                       .LineColor(orange);

                                    row.AutoItem()
                                       .Text(recipient.Name)
                                       .FontSize(32)
                                       .FontFamily("Damion")
                                       .FontColor(orange);
                                });
                        });

                        foreach (var entry in templatePostPairs)
                        {
                            container.Page(page =>
                            {
                                page.Size(PageSizes.Letter);
                                page.Margin(0.5f, Unit.Inch);
                                page.PageColor(white);

                                Page pageContent = entry.template switch
                                {
                                    TemplateA => new PageA(entry.posts),
                                    TemplateB => new PageB(entry.posts),
                                    TemplateC => new PageC(entry.posts),
                                    TemplateD => new PageD(entry.posts),
                                    TemplateE => new PageE(entry.posts),
                                    TemplateF => new PageF(entry.posts),
                                    TemplateG => new PageG(entry.posts),
                                    TemplateH => new PageH(entry.posts),
                                    TemplateI => new PageI(entry.posts),
                                    TemplateJ => new PageJ(entry.posts),
                                    _ => throw new InvalidOperationException($"Unknown template: {entry.template.GetType().Name}")
                                };

                                page.Content()
                                    .Component(pageContent);

                                page.Footer().Dynamic(new PageFooterComponent(new PageFooterComponentProps { Date = $"{issue.Issue.DraftingEnd:MMMM yyyy}" }));
                            });
                        }

                        int pages = templatePostPairs.Count + 2;
                        int remainder = pages % 4;

                        if (remainder != 0)
                        {
                            Log.Error("Need filler pages.");
                            for (int i = 0; i < 4 - remainder; i++)
                            {
                                Log.Error("Adding filler page...");
                                container.Page(page =>
                                {
                                    page.Size(PageSizes.Letter);
                                    page.Margin(0.5f, Unit.Inch);
                                    page.PageColor(white);
                                    page.Footer().Dynamic(new PageFooterComponent(new PageFooterComponentProps { Date = $"{issue.Issue.DraftingEnd:MMMM yyyy}" }));
                                });
                            }
                        }

                        container.Page(page =>
                        {
                            page.Size(PageSizes.Letter);
                            page.Margin(0.5f, Unit.Inch);
                            page.PageColor(white);

                            page.Content()
                                .Column(column =>
                                {
                                    column.Item()
                                          .PaddingTop(3.76f, Unit.Centimetre)
                                          .PaddingBottom(0.5f, Unit.Inch)
                                          .PaddingLeft(1.23f, Unit.Centimetre)
                                          .Text("Recipient")
                                          .FontColor(white)
                                          .FontSize(16)
                                          .FontFamily("Poppins")
                                          .Medium();

                                    column.Item()
                                          .PaddingLeft(1.23f, Unit.Centimetre)
                                          .Text(recipient.Name)
                                          .FontColor(charcoal800)
                                          .FontSize(12)
                                          .FontFamily("Poppins");

                                    column.Item()
                                          .PaddingLeft(1.23f, Unit.Centimetre)
                                          .Text(string.IsNullOrWhiteSpace(recipient.AddressLine2) ? recipient.AddressLine1 : $"{recipient.AddressLine1} {recipient.AddressLine2}")
                                          .FontColor(charcoal800)
                                          .FontSize(12)
                                          .FontFamily("Poppins");

                                    column.Item()
                                          .PaddingLeft(1.23f, Unit.Centimetre)
                                          .Text($"{recipient.City}, {recipient.ProvinceOrState} {recipient.PostalCode}")
                                          .FontColor(charcoal800)
                                          .FontSize(12)
                                          .FontFamily("Poppins");

                                    column.Item()
                                          .PaddingLeft(1.23f, Unit.Centimetre)
                                          .Text("USA")
                                          .FontColor(charcoal800)
                                          .FontSize(12)
                                          .FontFamily("Poppins");
                                });

                            page.Footer()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                       .AlignBottom()
                                       .Column(column =>
                                       {
                                           column.Item()
                                                 .PaddingBottom(0.05f, Unit.Inch)
                                                 .Height(0.5316f, Unit.Inch)
                                                 .Image("Assets\\Images\\logo.png")
                                                 .FitArea();

                                           column.Item()
                                                 .PaddingBottom(0.3f, Unit.Inch)
                                                 .Text("Standard copy")
                                                 .FontColor(orange)
                                                 .FontSize(18)
                                                 .FontFamily("Poppins")
                                                 .Medium();

                                           column.Item()
                                                 .Text("Copyright Hollow Inc.")
                                                 .FontColor("#868581")
                                                 .FontSize(12)
                                                 .FontFamily("Poppins");
                                       });

                                    row.RelativeItem()
                                       .AlignRight()
                                       .Height(2.5563f, Unit.Inch)
                                       .Image("Assets\\Images\\mouse.png")
                                       .FitArea();
                                });
                        });
                    }).GeneratePdf(memoryStream);

                    Log.Error($"Uploading PDF for {recipient.Name}.");
      
                    var shipMonth = DateTime.UtcNow.ToString("yyyy-MM");
                    var fileName = $"{issue.Issue.Title.Replace(" ", "_")}_{recipient.Name.Replace(" ", "_")}.pdf";

                    await UploadBlobAsync($"circles/{issue.CircleId}/issues/{issue.Issue.Id}/{fileName}", memoryStream);
                    Log.Error($"Uploaded to storage.");
                    await UploadBlobAsync($"shipping/{shipMonth}/{fileName}", memoryStream);
                    Log.Error($"Uploaded to shipping.");
                }

                Log.Error($"Marking issue {issue.Issue.Id} as published.");
                issue.Issue.Status = IssueStatus.Published;

                Issue toAdd = new()
                {
                    CircleId = issue.CircleId,
                    Title = $"{DateTime.UtcNow:MMMM yyyy} · Issue {issue.Issue.IssueNumber + 1}",
                    IssueNumber = issue.Issue.IssueNumber + 1,
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
