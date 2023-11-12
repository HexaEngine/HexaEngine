namespace HexaEngine.Core.Collections
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a key-value pair that implements the <see cref="INotifyPropertyChanged"/> interface.
    /// </summary>
    [Serializable]
    public class ObservableKeyValuePair<TKey, TValue> : INotifyPropertyChanged where TKey : notnull
    {
#nullable disable
        private TKey key;
        private TValue value;
#nullable restore
        /// <summary>
        /// Gets or sets the key of the key-value pair.
        /// </summary>
        public TKey Key
        {
            get { return key; }
            set
            {
                key = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the value of the key-value pair.
        /// </summary>
        public TValue Value
        {
            get { return value; }
            set
            {
                this.value = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc/>
        [field: NonSerialized]
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="name">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}