using CherAmiAPI.Interfaces;
using System;

namespace CherAmiAPI.Services
{
    public class PhotoDateService : IPhotoDateService
    {
        // Small allowance for client clock skew when comparing against "now".
        private static readonly TimeSpan FutureTolerance = TimeSpan.FromMinutes(5);

        public DateTimeOffset Normalize(DateTimeOffset? requested, DateTimeOffset draftingStart)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;

            if (requested == null) return now;
            if (requested < draftingStart) return now;
            if (requested > now + FutureTolerance) return now;

            return requested.Value;
        }
    }
}
