using Repository.Contexts;

namespace Repository.Repositories
{
    public abstract class Repository
    {
        internal readonly Func<LLContext> initContext;

        internal Repository(Func<LLContext> contextFactory)
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
