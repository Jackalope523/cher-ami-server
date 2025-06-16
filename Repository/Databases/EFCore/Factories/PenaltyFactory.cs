namespace Repository
{
    internal class PenaltyFactory : Factory
    {
        #region constructors
        public PenaltyFactory(IFactoryObserver observer) : base(observer)
        {

        }

        public PenaltyFactory(IEnumerable<IFactoryObserver> observers) : base(observers)
        {

        }

        public PenaltyFactory(params IFactoryObserver[] observers) : base(observers)
        {

        }
        #endregion

        internal Penalty Create(User user)
        {
            return Create(new Penalty
            {
                PenalizedId = user.Id,
                Type = PenaltyType.Unreliable,
                Time = DateTimeOffset.MinValue,
            });
        }
    }
}
