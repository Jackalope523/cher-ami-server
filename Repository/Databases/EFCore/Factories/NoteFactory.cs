
using Repository.Entities;

namespace Repository
{
    internal class NoteFactory : Factory
    {
        #region constructors
        public NoteFactory(IFactoryObserver observer) : base(observer)
        {

        }

        public NoteFactory(IEnumerable<IFactoryObserver> observers) : base(observers)
        {

        }

        public NoteFactory(params IFactoryObserver[] observers) : base(observers)
        {

        }
        #endregion

        internal Telegram Create(User Notifier, User Recipient)
        {
            return Create(new Telegram()
            {
                NotifierId = Notifier.Id,
                RecipientId = Recipient.Id,
                Time = DateTimeOffset.MinValue,
                Action = "Action " + Count(),
                Read = false
            });
        }
    }
}
