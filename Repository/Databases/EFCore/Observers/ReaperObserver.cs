
namespace Repository
{
    internal class ReaperObserver : IFactoryObserver
    {
        public List<Entity> Blacklist {  get; private set; }

        internal ReaperObserver()
        {
            Blacklist = new();
        }
        
        public void Notify(Entity entity)
        {
            Blacklist.Add(entity);
        }
        public void Notify(params Entity[] entities)
        {
            Blacklist.AddRange(entities);
        }
        public void Notify(IEnumerable<Entity> entities)
        {
            Blacklist.AddRange(entities);
        }
        public void Reap(EFCoreSentry sentry)
        {
            sentry.ExecuteWrite(ctx => ctx.RemoveRange(Blacklist));
        }
    }
}
