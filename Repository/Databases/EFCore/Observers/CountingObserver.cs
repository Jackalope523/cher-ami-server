
namespace Repository
{
    internal class CountingObserver : IFactoryObserver
    {
        public int Count { get; private set; }

        public void Notify(Entity entity)
        {
            Count++;
        }
        public void Notify(params Entity[] entities)
        {
            Count = Count + entities.Length;
        }
        public void Notify(IEnumerable<Entity> entities)
        {
            Count = Count + entities.Count();
        }
    }
}
