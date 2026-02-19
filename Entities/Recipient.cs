using System;

namespace CherAmiAPI.Entities
{
    public enum RecipientState
    { 
        Inactive, 
        Active 
    }

    public class Recipient
    {
        public long Id { get; set; }
        public string AvatarPath { get; set; }
        public DateTimeOffset? AvatarTimestamp { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string ProvinceOrState { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public long ManagerId { get; set; }
        public RecipientState State { get; set; }
        public bool SoftDeleted { get; set; }

        // Navigation Properties
        public User Manager { get; set; }
    }
}
