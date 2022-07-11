namespace FontEditor.MVVM.ViewModel
{
    using FontEditor.IO;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class BaseDataContext : INotifyPropertyChanged
    {
        private Uri currentTexture;

        public FontFile CurrentFont { get; set; }

        public Uri CurrentTexture { get => currentTexture; set { currentTexture = value; NotifyPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new(name));
        }
    }
}