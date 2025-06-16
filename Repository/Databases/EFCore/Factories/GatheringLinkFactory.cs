using Core.Boundaries;

namespace Repository
{
    internal class GatheringLinkFactory : Factory
    {
        #region constructors
        public GatheringLinkFactory(IFactoryObserver observer) : base(observer)
        {

        }

        public GatheringLinkFactory(IEnumerable<IFactoryObserver> observers) : base(observers)
        {

        }

        public GatheringLinkFactory(params IFactoryObserver[] observers) : base(observers)
        {

        }
        #endregion

        internal GatheringLink Create(User user, Gathering gathering, GatheringBond type)
        {
            return Create(new GatheringLink
            {
                UserId = user.Id,
                GatheringId = gathering.Id,
                Type = type,
                Time = DateTimeOffset.MinValue.AddHours(Count())
            });
        }
        internal GatheringLink Create(User user, Gathering gathering, GatheringBond type, DateTimeOffset time)
        {
            return Create(new GatheringLink
            {
                UserId = user.Id,
                GatheringId = gathering.Id,
                Type = type,
                Time = time
            });
        }

    }
}
