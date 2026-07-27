using System;

namespace CherAmiAPI.Interfaces
{
    public interface IPhotoDateService
    {
        /// <summary>
        /// A photo date must fall within its issue's drafting window and may not
        /// be in the future. Anything missing or out of range falls back to now.
        /// </summary>
        DateTimeOffset Normalize(DateTimeOffset? requested, DateTimeOffset draftingStart);
    }
}
