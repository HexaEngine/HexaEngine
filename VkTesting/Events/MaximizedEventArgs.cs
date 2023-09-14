namespace VkTesting.Events
{
    using VkTesting.Windows;

    public class MaximizedEventArgs : RoutedEventArgs
    {
        public WindowState OldState { get; internal set; }

        public WindowState NewState { get; internal set; }
    }
}