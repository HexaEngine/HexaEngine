namespace HexaEngine.Core
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Threading.Tasks;

    public interface IRenderDispatcher
    {
        void Dispose();

        void ExecuteQueue(IGraphicsContext immdiateContext);

        void Invoke(Action action);

        Task InvokeAsync(Action action);

        void InvokeBlocking(Action action);

        void InvokeOnehitDraw(Action<IGraphicsContext> action);

        Task InvokeOnehitDrawAsync(Action<IGraphicsContext> action);
    }
}