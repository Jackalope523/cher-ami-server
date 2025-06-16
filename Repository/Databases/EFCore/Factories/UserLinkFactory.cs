namespace Repository
{
    internal class UserLinkFactory : Factory
    {
        #region constructors
        public UserLinkFactory(IFactoryObserver observer) : base(observer)
        {

        }

        public UserLinkFactory(IEnumerable<IFactoryObserver> observers) : base(observers)
        {

        }

        public UserLinkFactory(params IFactoryObserver[] observers) : base(observers)
        {

        }
        #endregion

        internal UserRelationship Create(User self, User other, UserRelationship.UserRelationshipType type)
        {
            return Create(new UserRelationship
            {
                SelfId = self.Id,
                OtherId = other.Id,
                Type = type,
                Time = DateTimeOffset.MinValue
            });
        }
    }
}
