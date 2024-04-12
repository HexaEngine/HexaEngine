namespace HexaEngine.Core.Windows.Events
{
    public abstract class EventRoute
    {
        private readonly RoutedEvent routedEvent;

        public EventRoute(RoutedEvent routedEvent)
        {
            this.routedEvent = routedEvent;
        }

        public RoutedEvent RoutedEvent => routedEvent;

        public abstract void Invoke(RoutedEventArgs routedEvent);

        public abstract void Invoke(RoutedEventArgs routedEvent, object? stop);
    }

    public class EventRoute<TArgs> : EventRoute where TArgs : RoutedEventArgs
    {
        private readonly List<object> route = [];
        private readonly Dictionary<object, List<RoutedEventHandler<TArgs>>> targetToHandler = [];
        private readonly RoutedEvent<TArgs> routedEvent;

        public EventRoute(RoutedEvent<TArgs> routedEvent) : base(routedEvent)
        {
            this.routedEvent = routedEvent;
        }

        public void Add(object target, RoutedEventHandler<TArgs> handler)
        {
            if (targetToHandler.TryGetValue(target, out var eventHandlers))
            {
                eventHandlers.Add(handler);
                return;
            }

            route.Add(target);
            targetToHandler.Add(target, [handler]);
        }

        public void Remove(object target, RoutedEventHandler<TArgs> handler)
        {
            var handlers = targetToHandler[target];
            handlers.Remove(handler);
        }

        public override void Invoke(RoutedEventArgs routedEventArgs)
        {
            if (routedEventArgs is not TArgs args)
            {
                return;
            }

            switch (routedEvent.RoutingStrategy)
            {
                case RoutingStrategy.Tunnel:
                    {
                        for (int i = route.Count - 1; i >= 0; i--)
                        {
                            var target = route[i];
                            var handlers = targetToHandler[target];
                            foreach (var handler in handlers)
                            {
                                handler.Invoke(target, args);
                            }
                        }
                    }
                    break;

                case RoutingStrategy.Bubble:
                    {
                        for (int i = 0; i < route.Count; i++)
                        {
                            var target = route[i];
                            var handlers = targetToHandler[target];
                            foreach (var handler in handlers)
                            {
                                handler.Invoke(target, args);
                            }
                        }
                    }
                    break;

                case RoutingStrategy.Direct:
                    {
                        var directTarget = route[0];
                        var handlers = targetToHandler[directTarget];
                        foreach (var handler in handlers)
                        {
                            handler.Invoke(directTarget, args);
                        }
                    }
                    break;
            }
        }

        public override void Invoke(RoutedEventArgs routedEventArgs, object? stop)
        {
            if (routedEventArgs is not TArgs args)
            {
                return;
            }

            switch (routedEvent.RoutingStrategy)
            {
                case RoutingStrategy.Tunnel:
                    {
                        bool e = false;
                        for (int i = route.Count - 1; i >= 0; i--)
                        {
                            var target = route[i];

                            if (!e)
                            {
                                if (target == stop)
                                {
                                    e = true;
                                }
                                continue;
                            }

                            var handlers = targetToHandler[target];

                            foreach (var handler in handlers)
                            {
                                handler.Invoke(target, args);
                            }
                        }
                    }
                    break;

                case RoutingStrategy.Bubble:
                    {
                        for (int i = 0; i < route.Count; i++)
                        {
                            var target = route[i];

                            if (target == stop)
                            {
                                return;
                            }

                            var handlers = targetToHandler[target];
                            foreach (var handler in handlers)
                            {
                                handler.Invoke(target, args);
                            }
                        }
                    }
                    break;

                case RoutingStrategy.Direct:
                    {
                        var directTarget = route[0];
                        var handlers = targetToHandler[directTarget];
                        foreach (var handler in handlers)
                        {
                            handler.Invoke(directTarget, args);
                        }
                    }
                    break;
            }
        }
    }

    public abstract class RoutedEvent
    {
        public RoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
        {
            Name = name;
            RoutingStrategy = routingStrategy;
            HandlerType = handlerType;
            OwnerType = ownerType;
        }

        public string Name { get; set; }

        public RoutingStrategy RoutingStrategy { get; set; }

        public Type HandlerType { get; set; }

        public Type OwnerType { get; set; }

        public abstract bool IsHandlerOf(RoutedEventArgs args);

        public abstract EventRoute CreateEventRoute(object source);

        public abstract void AddToEventRoute(EventRoute eventRoute, object target);
    }

    public class RoutedEvent<TArgs> : RoutedEvent where TArgs : RoutedEventArgs
    {
        private readonly List<RoutedEventHandler<TArgs>> classHandlers = [];

        public RoutedEvent(string name, RoutingStrategy routingStrategy, Type ownerType) : base(name, routingStrategy, typeof(RoutedEventHandler<TArgs>), ownerType)
        {
        }

        public RoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType) : base(name, routingStrategy, handlerType, ownerType)
        {
        }

        public IReadOnlyList<RoutedEventHandler<TArgs>> ClassHandlers => classHandlers;

        public void AddClassHandler(RoutedEventHandler<TArgs> handler)
        {
            classHandlers.Add(handler);
        }

        public void AddClassHandler<TOwner>(Action<TOwner?, TArgs> action)
        {
            void handler(object? sender, TArgs args) { action.Invoke((TOwner?)sender, args); }
            classHandlers.Add(handler);
        }

        public override void AddToEventRoute(EventRoute eventRoute, object target)
        {
            if (eventRoute is not EventRoute<TArgs> route)
            {
                return;
            }

            for (int i = 0; i < classHandlers.Count; i++)
            {
                var classHandler = classHandlers[i];
                route.Add(target, classHandler);
            }
        }

        public override EventRoute CreateEventRoute(object source)
        {
            EventRoute<TArgs> eventRoute = new(this);
            for (int i = 0; i < classHandlers.Count; i++)
            {
                var classHandler = classHandlers[i];
                eventRoute.Add(source, classHandler);
            }

            return eventRoute;
        }

        public override bool IsHandlerOf(RoutedEventArgs args)
        {
            return args is TArgs;
        }
    }
}