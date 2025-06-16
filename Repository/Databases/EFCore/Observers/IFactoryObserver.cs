using Repository.Entities;

namespace Repository
{
    internal interface IFactoryObserver
    {
        public void Notify(Entity entity);
        public void Notify(params Entity[] entities);
        public void Notify(IEnumerable<Entity> entities);
    }
}
