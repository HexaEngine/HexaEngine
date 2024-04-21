namespace HexaEngine.Core
{
    using HexaEngine.Core.Windows.Events;
    using System;
    using System.Collections.Generic;

    public delegate void EventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e);

    public class EventHandlers<TEventArgs>
    {
        private readonly List<EventHandler<TEventArgs>> handlers = new();

        public void AddHandler(EventHandler<TEventArgs> handler)
        {
            handlers.Add(handler);
        }

        public void RemoveHandler(EventHandler<TEventArgs> handler)
        {
            handlers.Remove(handler);
        }

        public void Clear()
        {
            handlers.Clear();
        }

        public void Invoke(object? sender, TEventArgs args)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i](sender, args);
            }
        }

        public void InvokeRouted<TRoutedEventArgs>(object? sender, TRoutedEventArgs args) where TRoutedEventArgs : RoutedEventArgs, TEventArgs
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i](sender, args);
                if (args.Handled)
                {
                    return;
                }
            }
        }
    }

    public class EventHandlers<TSender, TEventArgs>
    {
        private EventHandler<TSender, TEventArgs>[]? handlers = null;
        private readonly object _lock = new();

        public void AddHandler(EventHandler<TSender, TEventArgs> handler)
        {
            lock (_lock)
            {
                if (handlers == null)
                {
                    handlers = [handler];
                    return;
                }

                Array.Resize(ref handlers, handlers.Length + 1);
                handlers[^1] = handler;
            }
        }

        public void RemoveHandler(EventHandler<TSender, TEventArgs> handler)
        {
            lock (_lock)
            {
                if (handlers == null) return;
                int index = Array.IndexOf(handlers, handler);
                if (index == -1) return;

                Array.Copy(handlers, index + 1, handlers, index, handlers.Length - index - 1);
                Array.Resize(ref handlers, handlers.Length - 1);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                if (handlers == null) return;
                for (int i = 0; i < handlers.Length; i++)
                {
                    handlers[i] = default;
                }
                handlers = null;
            }
        }

        public void Invoke(TSender sender, TEventArgs args)
        {
            lock (_lock)
            {
                if (handlers == null) return;
                for (int i = 0; i < handlers.Length; i++)
                {
                    handlers[i](sender, args);
                }
            }
        }
    }
}