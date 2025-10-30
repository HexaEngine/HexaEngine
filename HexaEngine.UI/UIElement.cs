namespace HexaEngine.UI
{
    using HexaEngine.Core.Windows.Events;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public partial class UIElement : Visual
    {
        private readonly List<EventRoute> eventRoutes = [];
        private bool isInitialized;

        public event EventHandler<EventArgs>? Initialized;

        public event EventHandler<EventArgs>? Uninitialized;

        public bool IsInitialized => isInitialized;

        public static readonly DependencyProperty<Visibility> VisibilityProperty = DependencyProperty.Register<UIElement, Visibility>(nameof(Visibility), false, new FrameworkMetadata(Visibility.Visible) { AffectsMeasure = true, AffectsArrange = true, AffectsRender = true });

        public Visibility Visibility
        {
            get => GetValue(VisibilityProperty);
            set => SetValue(VisibilityProperty, value);
        }

        public static readonly DependencyProperty<bool> IsEnabledProperty = DependencyProperty.Register<UIElement, bool>(nameof(IsEnabled), false, new FrameworkMetadata(true) { AffectsRender = true });

        public bool IsEnabled
        {
            get => GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty<bool> FocusableProperty = DependencyProperty.Register<UIElement, bool>(nameof(Focusable), false, new FrameworkMetadata(true) { AffectsRender = true });

        public bool Focusable
        {
            get => GetValue(FocusableProperty);
            set => SetValue(FocusableProperty, value);
        }

        public Vector2 RenderSize { get; set; }

        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnUninitialized()
        {
            Uninitialized?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Initialize()
        {
            foreach (var even in EventManager.GetRoutedEventsForOwnerIterator(DependencyObjectType))
            {
                var route = even.CreateEventRoute(this);
                BuildRoute(route);
                eventRoutes.Add(route);
            }

            isInitialized = true;
            OnInitialized();
            InitializeComponent();
        }

        private void BuildRoute(EventRoute route)
        {
            var current = Parent;
            while (current != null)
            {
                if (current is UIElement element)
                {
                    element.AddToEventRoute(route);
                }

                current = current.Parent;
            }
        }

        public void AddToEventRoute(EventRoute route)
        {
            foreach (var even in EventManager.GetRoutedEventsForOwnerIterator(DependencyObjectType))
            {
                if (route.RoutedEvent != even)
                {
                    continue;
                }
                even.AddToEventRoute(route, this);
            }
        }

        internal virtual void Uninitialize()
        {
            if (!isInitialized)
            {
                return;
            }
            foreach (var route in eventRoutes)
            {
                route.Clear();
            }
            eventRoutes.Clear();
            isInitialized = false;
            OnUninitialized();
            UninitializeComponent();
        }

        public virtual void InitializeComponent()
        {
        }

        public virtual void UninitializeComponent()
        {
        }

        public void AddHandler<T>(RoutedEvent<T> routedEvent, RoutedEventHandler<T> value) where T : RoutedEventArgs
        {
            for (int i = 0; i < eventRoutes.Count; i++)
            {
                var route = eventRoutes[i];
                if (route.RoutedEvent == routedEvent)
                {
                    ((EventRoute<T>)route).Add(this, value);
                }
            }
        }

        public void RemoveHandler<T>(RoutedEvent<T> routedEvent, RoutedEventHandler<T> value) where T : RoutedEventArgs
        {
            for (int i = 0; i < eventRoutes.Count; i++)
            {
                var route = eventRoutes[i];
                if (route.RoutedEvent == routedEvent)
                {
                    ((EventRoute<T>)route).Remove(this, value);
                }
            }
        }

        public void RaiseEvent(RoutedEventArgs args)
        {
            if (args.RoutedEvent == null)
            {
                return;
            }

            args.Source = this;

            for (int i = 0; i < eventRoutes.Count; i++)
            {
                var route = eventRoutes[i];
                if (route.RoutedEvent == args.RoutedEvent)
                {
                    route.Invoke(args);
                }
            }
        }

        public void RouteEvent(RoutedEventArgs args, object? stop)
        {
            if (args.RoutedEvent == null)
            {
                return;
            }

            args.Source = this;

            for (int i = 0; i < eventRoutes.Count; i++)
            {
                var route = eventRoutes[i];
                if (route.RoutedEvent == args.RoutedEvent)
                {
                    route.Invoke(args, stop);
                }
            }
        }
    }
}