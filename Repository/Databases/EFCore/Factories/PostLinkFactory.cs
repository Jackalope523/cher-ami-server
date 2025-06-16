
namespace Repository
{
    internal class PostLinkFactory : Factory
    {
        #region constructors
        public PostLinkFactory(IFactoryObserver observer) : base(observer)
        {

        }

        public PostLinkFactory(IEnumerable<IFactoryObserver> observers) : base(observers)
        {

        }

        public PostLinkFactory(params IFactoryObserver[] observers) : base(observers)
        {

        }
        #endregion

        internal SnapshotLink Create(User user, Snapshot snapshot)
        {
            return Create(new SnapshotLink
            {
                UserId = user.Id,
                SnapshotId = snapshot.Id,
                Type = SnapshotLink.SnapshotLinkType.Appreciate,
                Time = DateTimeOffset.MinValue
            });
        }
    }
}
