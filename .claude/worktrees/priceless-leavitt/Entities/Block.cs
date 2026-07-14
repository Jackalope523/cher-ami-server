using System;

namespace CherAmiAPI.Entities
{
    public class Block
    {
        public long Id { get; set; }
        public long BlockerId { get; set; }
        public long BlockedId { get; set; }
        public DateTimeOffset BlockDate { get; set; }
        public bool SoftDeleted { get; set; }

        // Navigation Properties
        public User Blocker { get; set; }
        public User Blocked { get; set; }
    }
}
