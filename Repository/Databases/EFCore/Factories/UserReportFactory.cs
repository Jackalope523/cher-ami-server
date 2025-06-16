namespace Repository
{
    internal class UserReportFactory : Factory
    {
        #region constructors
        public UserReportFactory(IFactoryObserver observer) : base(observer)
        {

        }

        public UserReportFactory(IEnumerable<IFactoryObserver> observers) : base(observers)
        {

        }

        public UserReportFactory(params IFactoryObserver[] observers) : base(observers)
        {

        }
        #endregion

        internal UserReport Create(User reporter, User reportee, Gathering location)
        {
            return Create(new UserReport
            {
                SelfId = reporter.Id,
                OtherId = reportee.Id,
                GatheringId = location.Id,
                Type = UserReportType.Rude,
                FilingDate = DateTimeOffset.MinValue,
                Notes = "Test User Report " + Count()
            });
        }
    }
}
