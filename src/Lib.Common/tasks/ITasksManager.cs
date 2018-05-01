#if EDDIENET4

using System;
using System.Threading;

namespace Eddie.Common.Tasks
{
    public interface ITasksManager : IDisposable
    {
        int Count { get; }
        TaskEx Add(Action<CancellationToken> action, bool start = true, CancellationTokenSource cancellation = null);
        void Remove(TaskEx task);
        void CancelAll();
        void WaitAll();
    }
}

#endif
