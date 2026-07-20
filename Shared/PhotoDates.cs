using System;

namespace CherAmiAPI.Shared
{
    public static class PhotoDates
    {
        // Small allowance for client clock skew when comparing against "now".
        private static readonly TimeSpan FutureTolerance = TimeSpan.FromMinutes(5);

        /// <summary>
        /// A photo date must fall within its issue's drafting window and may not
        /// be in the future. Anything missing or out of range falls back to now.
        /// </summary>
        public static DateTimeOffset Normalize(DateTimeOffset? requested, DateTimeOffset draftingStart)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;

            if (requested == null) return now;
            if (requested < draftingStart) return now;
            if (requested > now + FutureTolerance) return now;

            return requested.Value;
        }
    }
}
