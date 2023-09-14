namespace D3D12Testing.Input.Events
{
    public class KeyboardCharEventArgs : EventArgs
    {
        public KeyboardCharEventArgs()
        {
        }

        public char Char { get; internal set; }
    }
}