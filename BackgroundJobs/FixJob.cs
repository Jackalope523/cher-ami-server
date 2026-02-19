using CherAmiAPI.Contexts;
using CherAmiAPI.Endpoints.Users;
using CherAmiAPI.Entities;
using CherAmiAPI.Interfaces;
using CherAmiAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using Stripe;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace CherAmiAPI.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class FixJob(IServiceProvider _serviceProvider) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            HttpClient httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
            IKeyService keyService = scope.ServiceProvider.GetRequiredService<IKeyService>();
            CustomerService customerService = scope.ServiceProvider.GetRequiredService<CustomerService>();
            IImageService imageService = scope.ServiceProvider.GetRequiredService<IImageService>();

            Log.Error("Starting fixes.");
            Log.Error("Geeting posts from db.");
            List<Post> posts = await ctx.Posts.ToListAsync();

            foreach (Post post in posts)
            {
                //if (post.IssueId != 22)
                //{
                //    continue;
                //}
                Log.Error($"Loading post {post.Id}.");
                using Image image = Image.Load(await imageService.DownloadImageAsync(post.LowResolutionImagePath));
                
                double targetAspect = 372.0 / 259.0;
                double imageAspect = image.Width / (double)image.Height;

                if (Math.Abs(imageAspect - targetAspect) > 0.001)
                {
                    int cropWidth, cropHeight;

                    if (image.Width / (double)image.Height > targetAspect)
                    {
                        cropHeight = image.Height;
                        cropWidth = (int)(cropHeight * targetAspect);
                    }
                    else
                    {
                        cropWidth = image.Width;
                        cropHeight = (int)(cropWidth / targetAspect);
                    }

                    int cropX = 0; // left-aligned
                    int cropY = (image.Height - cropHeight) / 2; // vertically centered

                    image.Mutate(x =>
                        x.Crop(new Rectangle(cropX, cropY, cropWidth, cropHeight))
                    );

                    using MemoryStream memoryStream = new();
                    await image.SaveAsJpegAsync(memoryStream);
                    memoryStream.Position = 0;

                    await imageService.UploadImageAsync(post.LowResolutionImagePath, memoryStream);
                }
                else
                {
                    Log.Error($"Post {post.Id} fine.");
                }
            }

            Log.Error("Done running fixes.");
        }
    }
}
