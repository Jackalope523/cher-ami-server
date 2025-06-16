using Repository.Entities;

namespace Repository
{
    internal class SubscriptionFactory : Factory
    {
        #region constructors
        public SubscriptionFactory(IFactoryObserver observer) : base(observer)
        {

        }

        public SubscriptionFactory(IEnumerable<IFactoryObserver> observers) : base(observers)
        {

        }

        public SubscriptionFactory(params IFactoryObserver[] observers) : base(observers)
        {

        }
        #endregion

        internal Subscription Create(User user)
        {
            return Create(new Subscription
            {
                UserId = user.Id,
            });
        }
    }
}
