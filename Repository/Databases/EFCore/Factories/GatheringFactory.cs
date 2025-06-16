using Core.Boundaries;

namespace Repository
{
    internal class GatheringFactory : Factory
    {
        private readonly CoordinateFactory innerFactory = new();

        #region constructors
        public GatheringFactory(IFactoryObserver observer) : base(observer) 
        { 

        }

        public GatheringFactory(IEnumerable<IFactoryObserver> observers) : base(observers)
        {

        }

        public GatheringFactory(params IFactoryObserver[] observers) : base(observers)
        {

        }
        #endregion

        internal Gathering Create(User host)
        {
            return Create(new Gathering
            {
                Title = "gathering" + Count(),
                HostId = host.Id,
                Description = "This is gathering number " + Count() + ".",
                StartTime = DateTimeOffset.UtcNow.AddHours(Count()),
                GroupMinimum = 0 + Count(),
                GroupMaximum = 10 + Count(),
                State = GatheringState.Alive,
                Location = innerFactory.Create(17.544 + Count(), -72.483 - Count()),
                Radius = 10.0,
                IsDynamic = false,
                DegreeOfPrivacy = 3,

                Extroversion = 8 + Count(),
                Athleticisme = 2 + Count(),
                Openness = 6 + Count(),
                Chaos = 2 + Count(),
                Competitiveness = 7 + Count(),
                Industriousness = 3 + Count(),
                NightOwl = 9 + Count(),
            });
        }

        internal Gathering Create()
        {
            return Create(new Gathering
            {
                Title = "gathering" + Count(),
                HostId = 0,
                Description = "This is gathering number " + Count() + ".",
                StartTime = DateTimeOffset.UtcNow.AddHours(Count()),
                GroupMinimum = 0 + Count(),
                GroupMaximum = 10 + Count(),
                State = GatheringState.Alive,
                Location = innerFactory.Create(17.544 + Count(), -72.483 - Count()),
                Radius = 10.0,
                IsDynamic = false,
                DegreeOfPrivacy = 3,

                Extroversion = 8 + Count(),
                Athleticisme = 2 + Count(),
                Openness = 6 + Count(),
                Chaos = 2 + Count(),
                Competitiveness = 7 + Count(),
                Industriousness = 3 + Count(),
                NightOwl = 9 + Count(),
            });
        }
    }

}
