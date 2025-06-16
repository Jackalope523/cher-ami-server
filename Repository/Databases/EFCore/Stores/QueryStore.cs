
namespace Repository
{
    public abstract class QueryStore
    {
        internal IDatabaseSentry storeSentry;

        public QueryStore(Harbor.Flag flag)
        {
            storeSentry = new EFCoreSentry(flag);
        }      
    }
}

/*
    _.+._
  (^\/^\/^)
   \D*O*D/
   {_____}
           */
