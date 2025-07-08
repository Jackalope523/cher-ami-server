using Repository.Databases.Contexts;

namespace Repository.Databases.Stores
{
    public abstract class Repository
    {
        internal readonly Func<CanaryContext> initContext;

        internal Repository(Func<CanaryContext> contextFactory)
        {
            initContext = contextFactory;
        }
    }
}

/*
    _.+._
  (^\/^\/^)
   \D*O*D/
   {_____}
           */
