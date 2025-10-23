using System;

namespace CrazyLizard.Entities
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
        public DateTimeOffset AvatarTimestamp { get; set; }
        public string Title { get; set; } = DefaultTitle;
        public string FirstName { get; set; } = DefaultFirstName;
        public string LastName { get; set; } = DefaultLastName;
        public string UnitNumber { get; set; } = DefaultUnitNumber;
        public string Street { get; set; } = DefaultStreetAddress;
        public string City { get; set; } = DefaultCity;
        public string ProvinceOrState { get; set; } = DefaultProvinceOrState;
        public string PostalCode { get; set; } = DefaultPostalCode;
        public string Country { get; set; } = DefaultCountry;
        public long ManagerId { get; set; }
        public RecipientState State { get; set; }
        public bool SoftDeleted { get; set; }

        // Navigation Properties
        public User Manager { get; set; }

        // Default Values
        public static string DefaultTitle { get; set; } = "";
        public static string DefaultFirstName { get; set; } = "";
        public static string DefaultLastName { get; set; } = "";
        public static string DefaultUnitNumber { get; set; } = "";
        public static string DefaultStreetAddress { get; set; } = "";
        public static string DefaultCity { get; set; } = "";
        public static string DefaultProvinceOrState { get; set; } = "";
        public static string DefaultPostalCode { get; set; } = "";
        public static string DefaultCountry { get; set; } = "";
    }
}
