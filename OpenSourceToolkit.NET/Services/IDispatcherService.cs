using System;
using System.Threading.Tasks;

namespace OpenSourceToolkit.NET.Services
{
    /// <summary>
    /// Abstraction for UI thread dispatching.
    /// Allows ViewModels to be tested without Avalonia dependency.
    /// </summary>
    public interface IDispatcherService
    {
        /// <summary>
        /// Posts an action to the UI thread asynchronously.
        /// </summary>
        void Post(Action action);

        /// <summary>
        /// Invokes an action on the UI thread and waits for completion.
        /// </summary>
        Task InvokeAsync(Action action);

        /// <summary>
        /// Invokes a function on the UI thread and returns the result.
        /// </summary>
        Task<T> InvokeAsync<T>(Func<T> func);
    }

    /// <summary>
    /// Production implementation using Avalonia's Dispatcher.
    /// </summary>
    public class AvaloniaDispatcherService : IDispatcherService
    {
        public void Post(Action action)
        {
            global::Avalonia.Threading.Dispatcher.UIThread.Post(action);
        }

        public Task InvokeAsync(Action action)
        {
            return global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(action).GetTask();
        }

        public Task<T> InvokeAsync<T>(Func<T> func)
        {
            return global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(func).GetTask();
        }
    }

    /// <summary>
    /// Test implementation that executes actions synchronously on the calling thread.
    /// </summary>
    public class SynchronousDispatcherService : IDispatcherService
    {
        public void Post(Action action)
        {
            action?.Invoke();
        }

        public Task InvokeAsync(Action action)
        {
            action?.Invoke();
            return Task.CompletedTask;
        }

        public Task<T> InvokeAsync<T>(Func<T> func)
        {
            return Task.FromResult(func());
        }
    }
}
