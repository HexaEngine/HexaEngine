namespace HexaEngine.Input
{
    using Hexa.NET.Utilities;
    using HexaEngine.Input.Events;

    /// <summary>
    /// A thread-safe InputBuffer optimized for single-producer single-consumer.
    /// </summary>
    public class InputEventBuffer : IDisposable
    {
        private UnsafeCircularBuffer<InputEvent> inputQueue = new();
        private readonly object _lock = new();

        private uint maxDelay = 15;
        private int maxEvents = 10000;
        private InputEventType filter = InputEventType.None;
        private ulong lastTimestamp;
        private bool disposedValue;

        public InputEventBuffer()
        {
            inputQueue = new(maxEvents);
        }

        public InputEventBuffer(TimeSpan inputDelay, int maxEvents, InputEventType filter)
        {
            MaxDelay = inputDelay;
            MaxEvents = maxEvents;
            Filter = filter;
            MaxDelay = inputDelay;
            Filter = filter;
            inputQueue = new(maxEvents);
        }

        /// <summary>
        /// The maximum delay an event can have (default: 15ms) after the event is older than x it will be discarded.
        /// </summary>
        public TimeSpan MaxDelay { get => new(maxDelay); set => maxDelay = (uint)value.Milliseconds; }

        /// <summary>
        /// The maximum amount of events (default: 10000)
        /// </summary>
        public int MaxEvents
        {
            get => maxEvents; set
            {
                maxEvents = value;
                lock (_lock)
                {
                    inputQueue.Resize(value);
                }
            }
        }

        /// <summary>
        /// Allows to filter events, when set to <see cref="InputEventType.None"/> the filter is disabled.
        /// </summary>
        public InputEventType Filter { get => filter; set => filter = value; }

        /// <summary>
        /// The current count of elements in the queue. To access it thread-safe use <see cref="SyncObject"/>
        /// </summary>
        public int Count => inputQueue.Count;

        /// <summary>
        /// The sync object used in lock(SyncObject) expressions.
        /// </summary>
        public object SyncObject => _lock;

        /// <summary>
        /// Records a new event.
        /// </summary>
        /// <param name="inputEvent">The event.</param>
        public void RecordEvent(InputEvent inputEvent)
        {
            // doesn't need to lock here just doing filtering, early exit
            if (filter != InputEventType.None && (inputEvent.Type & filter) == 0)
            {
                return;
            }

            lock (_lock)
            {
                lastTimestamp = inputEvent.Timestamp;
                inputQueue.Enqueue(inputEvent);
            }
        }

        /// <summary>
        /// Polls an event from the queue.
        /// </summary>
        /// <param name="inputEvent">The polled event.</param>
        /// <returns>returns <see langword="true" /> if successful, otherwise <see langword="false" /> if the queue is <see langword="Empty" />.</returns>
        public bool PollEvent(ref InputEvent inputEvent)
        {
            lock (_lock)
            {
                while (inputQueue.TryDequeue(out inputEvent))
                {
                    if (lastTimestamp - inputEvent.Timestamp <= maxDelay)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool PeekEvent(ref InputEvent inputEvent)
        {
            lock (_lock)
            {
                while (inputQueue.TryPeek(out inputEvent))
                {
                    if (lastTimestamp - inputEvent.Timestamp <= maxDelay)
                    {
                        return true;
                    }
                    else
                    {
                        // discard event.
                        inputQueue.Dequeue();
                    }
                }
            }

            return false;
        }

        public void Clear()
        {
            lock (_lock)
            {
                inputQueue.Clear();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                inputQueue.Release();
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