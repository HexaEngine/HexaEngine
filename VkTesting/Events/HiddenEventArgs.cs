namespace VkTesting.Events
{
    using VkTesting.Windows;

    public class HiddenEventArgs : RoutedEventArgs
    {
        public WindowState OldState { get; internal set; }

        public WindowState NewState { get; internal set; }
    }
}