﻿namespace D3D12Testing.Events
{
    using D3D12Testing.Windows;

    public class MaximizedEventArgs : RoutedEventArgs
    {
        public WindowState OldState { get; internal set; }

        public WindowState NewState { get; internal set; }
    }
}