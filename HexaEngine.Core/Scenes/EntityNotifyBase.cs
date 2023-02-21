namespace HexaEngine.Core.Scenes
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class EntityNotifyBase : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangingEventHandler? PropertyChanging;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanging([CallerMemberName] string name = "")
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(name));
        }

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void SetAndNotify<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(name));
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected bool SetAndNotifyWithEqualsTest<T>(ref T field, T value, [CallerMemberName] string name = "") where T : IEquatable<T>
        {
            if (field.Equals(value)) return false;
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(name));
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            return true;
        }
    }
}