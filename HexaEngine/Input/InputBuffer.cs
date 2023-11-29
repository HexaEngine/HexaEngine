namespace HexaEngine.Input
{
    using HexaEngine.Input.Events;
    using System.Diagnostics;

    /// <summary>
    /// A thread-safe InputBuffer optimized for single-producer single-consumer.
    /// </summary>
    public class InputBuffer
    {
        private readonly Queue<InputEvent> inputQueue = new();
        private readonly object _lock = new();

        private long inputDelay = TimeSpan.FromMilliseconds(15).Ticks;
        private int maxEvents = 10000;
        private InputEventType filter = InputEventType.None;

        public InputBuffer()
        {
        }

        public InputBuffer(TimeSpan inputDelay, int maxEvents, InputEventType filter)
        {
            InputDelay = inputDelay;
            MaxEvents = maxEvents;
            Filter = filter;
            InputDelay = inputDelay;
            MaxEvents = maxEvents;
            Filter = filter;
        }

        /// <summary>
        /// The maximum delay an event can have (default: 15ms) after the event is older than x it will be discarded.
        /// </summary>
        public TimeSpan InputDelay { get => new(inputDelay); set => inputDelay = value.Ticks; }

        /// <summary>
        /// The maximum amount of events (default: 10000)
        /// </summary>
        public int MaxEvents { get => maxEvents; set => maxEvents = value; }

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
                return;

            lock (_lock)
            {
                if (inputQueue.Count >= maxEvents)
                    return;

                inputEvent.Timestamp = Stopwatch.GetTimestamp();
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
                if (!inputQueue.TryDequeue(out inputEvent))
                {
                    return false;
                }
            }

            // doesn't need to lock here it's just calculus where the producer can still add new events to the end of the queue.
            var currentTime = Stopwatch.GetTimestamp();
            if (currentTime - inputEvent.Timestamp <= inputDelay)
            {
                return true;
            }
            else
            {
                return PollEvent(ref inputEvent);
            }
        }

        public bool PeekEvent(ref InputEvent inputEvent)
        {
            lock (_lock)
            {
                if (!inputQueue.TryPeek(out inputEvent))
                {
                    return false;
                }

                var currentTime = Stopwatch.GetTimestamp();
                if (currentTime - inputEvent.Timestamp <= inputDelay)
                {
                    return true;
                }
                else
                {
                    // discard event.
                    inputQueue.Dequeue();

                    return PeekEvent(ref inputEvent);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                inputQueue.Clear();
            }
        }
    }
}