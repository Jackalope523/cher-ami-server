using Repository.Databases.Entities;

namespace Repository.Databases.Entities.Messages
{
    public class ProfileMessage : Message
    {
        public long ProfileId { get; set; }

        // Navigation Properties
        public User? Profile { get; set; }
    }
}
