namespace HexaEngine.Core.Windows
{
    using System;
    using System.Threading.Tasks;

    public interface IThreadDispatcher
    {
        void Dispose();

        void Invoke(Action action);

        Task InvokeAsync(Action action);

        void InvokeBlocking(Action action);

        void Invoke(Action<object> action, object state);

        Task InvokeAsync(Action<object> action, object state);

        void InvokeBlocking(Action<object> action, object state);
    }
}