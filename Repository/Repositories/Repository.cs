using Repository.Contexts;

namespace Repository.Repositories
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
