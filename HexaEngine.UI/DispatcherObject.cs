namespace HexaEngine.UI
{
    using Hexa.NET.Utilities.Threading;
    using HexaEngine.Core;

    public class DispatcherObject
    {
        public IThreadDispatcher Dispatcher { get; } = Application.MainWindow.Dispatcher;
    }
}