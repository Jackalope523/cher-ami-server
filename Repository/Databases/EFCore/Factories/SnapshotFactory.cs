
namespace Repository
{
    internal class SnapshotFactory : Factory
    {
        #region constructors
        public SnapshotFactory(IFactoryObserver observer) : base(observer)
        {

        }

        public SnapshotFactory(IEnumerable<IFactoryObserver> observers) : base(observers)
        {

        }

        public SnapshotFactory(params IFactoryObserver[] observers) : base(observers)
        {

        }
        #endregion

        internal Snapshot Create(User owner, Gathering location)
        {
            return Create(new Snapshot
            {
                OwnerId = owner.Id,
                GatheringId = location.Id,
                PostedAt = DateTime.MinValue,
            });
        }
        internal Snapshot Create(User owner, Gathering location, DateTimeOffset postedAt)
        {
            return Create(new Snapshot
            {
                OwnerId = owner.Id,
                GatheringId = location.Id,
                PostedAt = postedAt,
            });
        }
    }
}
