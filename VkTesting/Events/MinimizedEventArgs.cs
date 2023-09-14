namespace VkTesting.Events
{
    using VkTesting.Windows;

    public class MinimizedEventArgs : RoutedEventArgs
    {
        public WindowState OldState { get; internal set; }

        public WindowState NewState { get; internal set; }
    }
}