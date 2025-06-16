namespace Repository
{
    internal class GatheringReportFactory : Factory
    {
        #region constructors
        public GatheringReportFactory(IFactoryObserver observer) : base(observer)
        {

        }

        public GatheringReportFactory(IEnumerable<IFactoryObserver> observers) : base(observers)
        {

        }

        public GatheringReportFactory(params IFactoryObserver[] observers) : base(observers)
        {

        }
        #endregion

        internal GatheringReport Create(User reporter, Gathering location)
        {
            return Create(new GatheringReport
            {
                UserId = reporter.Id,
                GatheringId = location.Id,
                Type = GatheringReportType.Misleading,
                FilingDate = DateTimeOffset.MinValue,
                Notes = "Test Gathering Report " + Count()
            });          
        }
    }
}
