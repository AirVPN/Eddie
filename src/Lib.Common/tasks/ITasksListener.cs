#if EDDIENET4

namespace Eddie.Common.Tasks
{
    public interface ITasksListener
    {
        // Callback invoked at the end of a task (canceled or not, test the property "Canceled" if needed)
        void onTaskEnded(TaskEx task);
    }
}

#endif
