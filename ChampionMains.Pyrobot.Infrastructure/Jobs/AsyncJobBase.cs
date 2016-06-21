using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Hallam.RedditRankedFlairs.Jobs
{
    public abstract class AsyncJobBase
    {
        protected virtual void ExecuteInternal(Task job)
        {
            try
            {
                job.Wait();
            }
            catch (AggregateException e)
            {
                Exception inner = e.InnerException;
                ExceptionDispatchInfo.Capture(inner).Throw();
            }
        }
    }
}