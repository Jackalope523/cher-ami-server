using System.Xml.Serialization;

namespace Repository
{
    internal abstract class Factory
    {
        private CountingObserver _counter;
        private List<IFactoryObserver> subscribers;

        #region constructors
        internal Factory() 
        {
            _counter = new CountingObserver();
            subscribers = new() { _counter };
        }
        internal Factory(IFactoryObserver observer) : this()
        {
            subscribers.Add(observer);
        }
        internal Factory(IEnumerable<IFactoryObserver> observers) : this()
        {
            subscribers.AddRange(observers);
        }
        internal Factory(params IFactoryObserver[] observers) : this()
        {
            subscribers.AddRange(observers);
        }
        #endregion

        #region methods
        internal void Assign(IFactoryObserver observer)
        {
            subscribers.Add(observer);
        }
        internal void Dismiss(IFactoryObserver observer)
        {
            subscribers.Remove(observer);
        }
        internal void NotifyObservers(Entity entity)
        {
            foreach (IFactoryObserver observer in subscribers)
            {
                observer.Notify(entity);
            }
        }
        internal int Count()
        {
            return _counter.Count;
        }
        internal T Create<T>(T entity) where T : Entity
        {
            NotifyObservers(entity);
            return entity;
        }
        #endregion
    }
}
