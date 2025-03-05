namespace HexaEngine.Jobs
{
    using System.Threading;

    public class Job : IAsyncResult, IDisposable
    {
        internal ManualResetEventSlim? completionEvent;
        internal JobState state;

        internal Job? parent;

        internal readonly Action<object?> action;
        internal object? stateObject;

        private bool disposedValue;

        public string? Name { get; }

        public JobFlags Flags { get; }

        public JobPriority Priority { get; }

        public JobState State => state;

        public Action<object?> Action => action;

        public bool IsCompleted => state >= JobState.CompletedSuccessfully;

        public Exception? Exception { get; internal set; }

        public bool IsCompletedSuccessfully => state == JobState.CompletedSuccessfully;

        public object? AsyncState => stateObject;

        public WaitHandle AsyncWaitHandle => completionEvent?.WaitHandle ?? throw new Exception("Job was not created yet.");

        public bool CompletedSynchronously => state == JobState.CompletedSynchronously;

        public static readonly Job JobCompleted = Run(() => { }, JobPriority.Highest, JobFlags.None);

        public Job(string? name, Action action, JobPriority priority, JobFlags flags)
        {
            Name = name;
            Flags = flags;
            Priority = priority;
            this.action = state => action();
        }

        public Job(Action action, JobPriority priority, JobFlags flags)
        {
            Name = null;
            Flags = flags;
            Priority = priority;
            this.action = state => action();
        }

        public Job(Action action, JobPriority priority)
        {
            Name = null;
            Flags = JobFlags.None;
            Priority = priority;
            this.action = state => action();
        }

        public Job(Action action)
        {
            Name = null;
            Flags = JobFlags.None;
            Priority = JobPriority.Normal;
            this.action = state => action();
        }

        public Job(string? name, object? state, Action<object?> action, JobPriority priority, JobFlags flags)
        {
            Name = name;
            Flags = flags;
            Priority = priority;
            this.action = action;
            stateObject = state;
        }

        public Job(object? state, Action<object?> action, JobPriority priority, JobFlags flags)
        {
            Name = null;
            Flags = flags;
            Priority = priority;
            this.action = action;
            stateObject = state;
        }

        public Job(object? state, Action<object?> action, JobPriority priority)
        {
            Name = null;
            Flags = JobFlags.None;
            Priority = priority;
            this.action = action;
            stateObject = state;
        }

        public Job(object? state, Action<object?> action)
        {
            Name = null;
            Flags = JobFlags.None;
            Priority = JobPriority.Normal;
            this.action = action;
            stateObject = state;
        }

        public void Start()
        {
            JobScheduler.Default.Run(this);
        }

        public static Job Run(Action action, JobPriority priority = JobPriority.Normal, JobFlags flags = JobFlags.None)
        {
            return JobScheduler.Default.StartNew(action, priority, flags);
        }

        public static Job Run(object? state, Action<object?> action, JobPriority priority = JobPriority.Normal, JobFlags flags = JobFlags.None)
        {
            return JobScheduler.Default.StartNew(state, action, priority, flags);
        }

        public static Job Run(string? name, Action action, JobPriority priority = JobPriority.Normal, JobFlags flags = JobFlags.None)
        {
            return JobScheduler.Default.StartNew(name, action, priority, flags);
        }

        public static Job Run(string? name, object? state, Action<object?> action, JobPriority priority = JobPriority.Normal, JobFlags flags = JobFlags.None)
        {
            return JobScheduler.Default.StartNew(name, state, action, priority, flags);
        }

        internal void SignalCompletion()
        {
            completionEvent?.Set();
        }

        public void Wait()
        {
            if (completionEvent == null)
            {
                throw new Exception("Job was not created yet.");
            }

            completionEvent.Wait();
        }

        public static void WaitAll(IList<Job> jobs)
        {
            foreach (Job job in jobs)
            {
                job.Wait();
            }
        }

        public void Reset()
        {
            state = JobState.NotCreated;
        }

        public void Reset(object? stateObject)
        {
            this.stateObject = stateObject;
            state = JobState.NotCreated;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                completionEvent?.Dispose();
                completionEvent = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}