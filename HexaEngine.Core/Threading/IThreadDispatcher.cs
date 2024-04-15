namespace HexaEngine.Core.Threading
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an interface for dispatching actions to a specific thread or execution context.
    /// </summary>
    public interface IThreadDispatcher : IDisposable
    {
        /// <summary>
        /// Invokes the specified action on the associated thread or execution context.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        void Invoke(Action action);

        /// <summary>
        /// Asynchronously invokes the specified action on the associated thread or execution context.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task InvokeAsync(Action action);

        EventWaitHandle? InvokeWaitHandle(Action action);

        /// <summary>
        /// Invokes the specified action on the associated thread or execution context, blocking the calling thread until the action completes.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        void InvokeBlocking(Action action);

        /// <summary>
        /// Invokes the specified action on the associated thread or execution context with additional state information.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="state">An object that contains data to be used by the action.</param>
        void Invoke(Action<object> action, object state);

        /// <summary>
        /// Asynchronously invokes the specified action on the associated thread or execution context with additional state information.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="state">An object that contains data to be used by the action.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task InvokeAsync(Action<object> action, object state);

        EventWaitHandle? InvokeWaitHandle(Action<object> action, object state);

        /// <summary>
        /// Invokes the specified action on the associated thread or execution context with additional state information, blocking the calling thread until the action completes.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="state">An object that contains data to be used by the action.</param>
        void InvokeBlocking(Action<object> action, object state);

        Task InvokeAsync<T>(Action<T> action, T state);

        EventWaitHandle? InvokeWaitHandle<T>(Action<T> action, T state);

        void InvokeBlocking<T>(Action<T> action, T state);
    }
}