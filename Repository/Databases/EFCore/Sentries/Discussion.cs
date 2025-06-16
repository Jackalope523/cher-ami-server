
using System.Runtime.CompilerServices;

namespace Repository
{
    internal class Discussion
    {
        internal CanaryContext SharedContext { get; private set; }

        internal Discussion(CanaryContext sharedContext)
        {
            SharedContext = sharedContext;
        }

        internal void End()
        {
            try
            {
                SharedContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                SharedContext.Dispose();
            }                
        }

        internal async Task  EndAsync()
        {
            try
            {
                await SharedContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                await SharedContext.DisposeAsync();
            }
        }

        internal void EndNow()
        {
            SharedContext.Dispose();
        }
    }
}
