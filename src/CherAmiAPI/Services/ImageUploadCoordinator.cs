using Serilog;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CherAmiAPI.Services
{
    public class ImageUploadCoordinator
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _uploads = new();
        private static readonly TimeSpan CleanupDelay = TimeSpan.FromMinutes(5);

        public Task WaitForUploadAsync(string imageId)
        {
            var tcs = _uploads.GetOrAdd(imageId, id => 
            {
                var newTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                _ = ScheduleCleanup(id);
                return newTcs;
            });
            return tcs.Task;
        }

        public void MarkUploaded(string imageId)
        {
            var tcs = _uploads.GetOrAdd(imageId, id => 
            {
                var newTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                _ = ScheduleCleanup(id);
                return newTcs;
            });
            tcs.TrySetResult(true);
        }

        private async Task ScheduleCleanup(string imageId)
        {
            try
            {
                await Task.Delay(CleanupDelay);

                if (_uploads.TryRemove(imageId, out var tcs))
                {
                    tcs.TrySetCanceled();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Cleanup failed for {imageId}: {ex}");
            }
        }
    }
}
