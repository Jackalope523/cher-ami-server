namespace Repository
{
    internal class UserFactory : Factory
    {
        private CoordinateFactory internalFactory = new();

        #region constructors
        public UserFactory(IFactoryObserver observer) : base(observer)
        {

        }

        public UserFactory(IEnumerable<IFactoryObserver> observers) : base(observers)
        {

        }

        public UserFactory(params IFactoryObserver[] observers) : base(observers)
        {

        }
        #endregion


        internal User Create()
        {
            return Create(new User
            {
                PhoneNumber = "1" + new string((char)Count(), 10),
                Email = "email_" + Count() + "@test.com",
                Name = "user" + Count(),
                DateOfBirth = DateTimeOffset.Now.AddDays(-(Count() + 1000)),
                JoinDate = DateTimeOffset.Now.AddDays(-365),
                Reputation = 100 - Count(),
                NormalisedEmail = "email_" + Count() + "@test.com",
                IsPhoneConfirmed = false,
                IsEmailConfirmed = false,
                SecurityStamp = "stamp" + Count(),
                LockoutDate = DateTimeOffset.Now.AddDays(-365),
                AccessTries = 3 + Count(),
                AccountStatus = UserAccountStatus.Active,
                NotificationId = Guid.NewGuid(),

                Extroversion = 5 + Count(),
                Athleticisme = 7 + Count(),
                Openness = 3 + Count(),
                Chaos = 5 + Count(),
                Competitiveness = 6 + Count(),
                Industriousness = 1 + Count(),
                NightOwl = 9 + Count(),

                Haunt = internalFactory.Create(40.712, -74.006),
                HauntRadius = 10.0 + Count(),
                HauntWeight = 5 + Count(),
                CurrentLocation = internalFactory.Create(40.712, -74.006),
                CurrentRadius = 15.0 + Count()
            });
        }
    }
}
