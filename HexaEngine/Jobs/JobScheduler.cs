namespace HexaEngine.Jobs
{
    using System.Collections.Concurrent;
    using System.Threading;

    public class JobScheduler
    {
        private readonly List<ConcurrentQueue<Job>> jobQueues = [];
        private readonly List<Job> jobs = [];
        private readonly object _lock = new();

        private readonly int jobQueuesCount = 0;
        private int jobsCount = 0;

        private readonly int threadsCount;
        private readonly JobWorker[] workers;

        private readonly List<LongRunningJobWorker> longRunningWorkers = [];

        private bool running;

        public JobScheduler()
        {
            threadsCount = Environment.ProcessorCount > 6 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1;
            running = true;

            jobQueuesCount = Enum.GetValues<JobPriority>().Length;
            for (int i = 0; i < jobQueuesCount; i++)
            {
                jobQueues.Add(new ConcurrentQueue<Job>());
            }

            workers = new JobWorker[threadsCount];
            for (int i = 0; i < threadsCount; i++)
            {
                Thread thread = new(WorkerVoid);
                JobWorker worker = new(thread, i);
                thread.Start(worker);
                workers[i] = worker;
            }
        }

        public static JobScheduler Default { get; } = new();

        public IReadOnlyList<Job> Jobs => jobs;

        public object SyncObject => _lock;

        private void SignalAny()
        {
            lock (_lock)
            {
                for (int i = 0; i < threadsCount; i++)
                {
                    var worker = workers[i];
                    if (worker.Idle)
                    {
                        worker.Signal();
                        return;
                    }
                }
            }
        }

        public void Run(Job job)
        {
            job.completionEvent = new();
            job.state = JobState.Created;

            if (job.Flags.HasFlag(JobFlags.LongRunning))
            {
                Thread thread = new(LongRunningWorkerVoid);
                lock (_lock)
                {
                    LongRunningJobWorker worker = new(thread, longRunningWorkers.Count);
                    longRunningWorkers.Add(worker);
                    thread.Start(job);
                }
            }
            else
            {
                ConcurrentQueue<Job> queue = jobQueues[(int)job.Priority];
                queue.Enqueue(job);

                lock (_lock)
                {
                    jobs.Add(job);
                }

                Interlocked.Increment(ref jobsCount);

                SignalAny();
            }
        }

        public Job StartNew(string? name, object? state, Action<object?> action, JobPriority priority = JobPriority.Normal, JobFlags flags = JobFlags.None)
        {
            Job job = new(name, state, action, priority, flags);
            Run(job);
            return job;
        }

        public Job StartNew(string? name, Action action, JobPriority priority = JobPriority.Normal, JobFlags flags = JobFlags.None)
        {
            Job job = new(name, action, priority, flags);
            Run(job);
            return job;
        }

        public Job StartNew(object? state, Action<object?> action, JobPriority priority = JobPriority.Normal, JobFlags flags = JobFlags.None)
        {
            Job job = new(state, action, priority, flags);
            Run(job);
            return job;
        }

        public Job StartNew(Action action, JobPriority priority = JobPriority.Normal, JobFlags flags = JobFlags.None)
        {
            Job job = new(action, priority, flags);
            Run(job);
            return job;
        }

        public List<Job> GetAllJobsWithFlag(JobFlags flags)
        {
            List<Job> jobs = [];
            lock (_lock)
            {
                for (int i = 0; i < this.jobs.Count; i++)
                {
                    Job job = this.jobs[i];
                    if (job.Flags.HasFlag(flags))
                    {
                        jobs.Add(job);
                    }
                }
            }
            return jobs;
        }

        private class JobWorker
        {
            public readonly Thread Thread;
            public readonly int Id;
            public readonly AutoResetEvent Handle;
            public volatile bool Idle;

            public JobWorker(Thread thread, int id)
            {
                Thread = thread;
                Id = id;
                Handle = new(false);
            }

            public void Dispose()
            {
                Handle.Set();
                Thread.Join();
                Handle.Dispose();
            }

            public void Signal()
            {
                Handle.Set();
            }
        }

        private class LongRunningJobWorker
        {
            public readonly Thread Thread;
            public readonly int Id;

            public LongRunningJobWorker(Thread thread, int id)
            {
                Thread = thread;
                Id = id;
            }

            public void Dispose()
            {
                Thread.Join();
            }
        }

        private static void LongRunningWorkerVoid(object? param)
        {
            Job job = (Job?)param ?? throw new ArgumentNullException(nameof(param));
            job.state = JobState.Running;
            try
            {
                job.Action(job.stateObject);
                job.state = JobState.CompletedSuccessfully;
            }
            catch (Exception ex)
            {
                job.Exception = ex;
                job.state = JobState.Faulted;
            }

            job.SignalCompletion();
        }

        private Job? GetJob()
        {
            for (int i = jobQueuesCount - 1; i >= 0; i--)
            {
                var queue = jobQueues[i];
                if (queue.TryDequeue(out var job))
                {
                    Interlocked.Decrement(ref jobsCount);
                    return job;
                }
            }

            return null;
        }

        private void WorkerVoid(object? param)
        {
            JobWorker worker = (JobWorker?)param ?? throw new ArgumentNullException(nameof(param));

            while (running)
            {
                worker.Idle = true;
                worker.Handle.WaitOne();
                worker.Idle = false;

                if (!running)
                {
                    return;
                }

                while (jobsCount > 0)
                {
                    Job? job = GetJob();

                    if (job == null)
                    {
                        continue;
                    }

                    job.state = JobState.Running;

                    try
                    {
                        job.Action(job.stateObject);
                        job.state = JobState.CompletedSuccessfully;
                    }
                    catch (Exception ex)
                    {
                        job.Exception = ex;
                        job.state = JobState.Faulted;
                    }

                    job.SignalCompletion();

                    lock (jobs)
                    {
                        jobs.Remove(job);
                    }
                }
            }
        }

        public void Dispose()
        {
            running = false;

            for (int i = 0; i < threadsCount; i++)
            {
                workers[i].Dispose();
            }

            for (int i = 0; i < longRunningWorkers.Count; i++)
            {
                longRunningWorkers[i].Dispose();
            }
        }
    }
}