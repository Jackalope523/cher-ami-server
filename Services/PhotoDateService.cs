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

            // A new circle's DraftingStart is its creation time, but its magazine covers the whole month.
            DateTimeOffset earliest = new(new DateTime(draftingStart.Year, draftingStart.Month, 1), TimeSpan.Zero);

            if (requested == null) return now;
            if (requested < earliest) return now;
            if (requested > now + FutureTolerance) return now;

            return requested.Value;
        }
    }
}
