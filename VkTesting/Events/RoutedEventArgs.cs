﻿namespace VkTesting.Events
{
    using System;

    public class RoutedEventArgs : EventArgs
    {
        /// <summary>
        /// Supresses the event and restores original values.
        /// </summary>
        public bool Handled { get; set; }
    }
}