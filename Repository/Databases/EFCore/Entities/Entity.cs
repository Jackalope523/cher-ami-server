namespace Repository
{
    public abstract class Entity
    {
        public long Id { get; set; } = DefaultId;
        public bool SoftDeleted { get; set; } = DefaultSoftDeleted;

        public static long DefaultId { get; set; } = 0;
        public static bool DefaultSoftDeleted { get; set; } = false;
    }
}
