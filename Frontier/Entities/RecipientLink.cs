using System;

namespace Repository.Entities
{
    public class RecipientLink : Entity
    {
        public long RecipientId { get; set; }
        public long CircleId { get; set; }
        public DateTimeOffset JoinDate { get; set; }

        // Navigation Properties
        public Recipient Recipient { get; set; }
        public Circle Circle { get; set; }
    }
}
