
namespace Repository
{
    public abstract class QueryStore
    {
        internal readonly Func<CanaryContext> initContext;

        internal QueryStore(Func<CanaryContext> contextFactory)
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
