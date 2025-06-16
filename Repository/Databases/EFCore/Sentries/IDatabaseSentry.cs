namespace Repository
{
    internal interface IDatabaseSentry
    {
        internal T ExecuteRead<T>(Func<CanaryContext, T> read);
        internal void ExecuteWrite(Action<CanaryContext> write);

        internal Task<T> ExecuteReadAsync<T>(Func<CanaryContext, Task<T>> read);
        internal Task ExecuteWriteAsync(Action<CanaryContext> write);
        internal Task ExecuteWriteAsync(Func<CanaryContext, Task> write);

        internal Discussion BeginDiscussion();
        internal void DiscussWrite(Action<CanaryContext> write, Discussion discussion);
        internal T DiscussRead<T>(Func<CanaryContext, T> read, Discussion discussion);
        internal Task DiscussWriteAsync(Func<CanaryContext, Task> write, Discussion discussion);
        internal void EndDiscussion(Discussion toEnd);
        internal Task EndDiscussionAsync(Discussion toEnd);
    }
}