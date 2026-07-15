using System;

namespace CherAmiAPI.Entities
{
    internal class EmailLogin
    { 
        public long Id { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public bool SoftDeleted { get; set; }
    }
}
