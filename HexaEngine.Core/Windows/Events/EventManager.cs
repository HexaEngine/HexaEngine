namespace HexaEngine.Core.Windows.Events
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    public static class EventManager
    {
        private static readonly List<RoutedEvent> routedEvents = [];
        private static readonly Dictionary<(string, Type), RoutedEvent> handlerTypeToEvent = [];
        private static readonly Dictionary<Type, List<RoutedEvent>> ownerTypeToEvent = [];

        public static IReadOnlyList<RoutedEvent> GetRoutedEvents() => routedEvents;

        public static IReadOnlyList<RoutedEvent> GetRoutedEventsForOwner(Type ownerType)
        {
            if (ownerTypeToEvent.TryGetValue(ownerType, out List<RoutedEvent>? events))
            {
                return events;
            }
            return [];
        }

        public static IEnumerable<RoutedEvent> GetRoutedEventsForOwnerIterator(Type ownerType)
        {
            for (int i = 0; i < routedEvents.Count; i++)
            {
                var routedEvent = routedEvents[i];
                if (routedEvent.OwnerType.IsAssignableFrom(ownerType))
                {
                    yield return routedEvent;
                }
            }
        }

        public static IReadOnlyList<RoutedEvent> GetRoutedEventsForOwner<T>()
        {
            if (ownerTypeToEvent.TryGetValue(typeof(T), out List<RoutedEvent>? events))
            {
                return events;
            }
            return [];
        }

        public static RoutedEvent<TArgs> Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicEvents)] TOwner, TArgs>(string name, RoutingStrategy routingStrategy) where TArgs : RoutedEventArgs
        {
            Type ownerType = typeof(TOwner);
            EventInfo eventInfo = ownerType.GetEvent(name) ?? throw new ArgumentException($"Couldn't find event '{name}' in '{ownerType}'");
            Type handlerType = typeof(RoutedEventHandler<TArgs>);
            (string, Type) key = (name, handlerType);

            if (handlerTypeToEvent.TryGetValue(key, out var routedEvent))
            {
                AddOwner(routedEvent, ownerType);
                return (RoutedEvent<TArgs>)routedEvent;
            }

            RoutedEvent<TArgs> routedEventGeneric = new(name, routingStrategy, handlerType, ownerType);
            routedEvents.Add(routedEventGeneric);

            AddOwner(routedEventGeneric, ownerType);

            return routedEventGeneric;
        }

        private static void AddOwner(RoutedEvent routedEvent, Type ownerType)
        {
            if (!ownerTypeToEvent.TryGetValue(ownerType, out var events))
            {
                events = [];
                ownerTypeToEvent.Add(ownerType, events);
            }

            events.Add(routedEvent);
        }
    }
}