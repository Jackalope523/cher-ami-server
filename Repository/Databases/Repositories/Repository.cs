using Repository.Databases.Contexts;

namespace Repository.Databases.Stores
{
    public abstract class Repository
    {
        internal readonly Func<CardinalContext> initContext;

        internal Repository(Func<CardinalContext> contextFactory)
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
