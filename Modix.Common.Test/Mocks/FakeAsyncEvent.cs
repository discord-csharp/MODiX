using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modix.Common.Test.Mocks
{
    public class FakeAsyncEvent
    {
        public IReadOnlyCollection<Func<Task>> Handlers
            => _handlers;

        public void AddHandler(Func<Task> handler)
            => _handlers.Add(handler);

        public void RemoveHandler(Func<Task> handler)
            => _handlers.Remove(handler);

        public async Task InvokeAsync()
        {
            foreach (var handler in _handlers)
                await handler.Invoke();
        }

        private readonly List<Func<Task>> _handlers
            = new List<Func<Task>>();
    }

    public class FakeAsyncEvent<T>
    {
        public IReadOnlyCollection<Func<T, Task>> Handlers
            => _handlers;

        public void AddHandler(Func<T, Task> handler)
            => _handlers.Add(handler);

        public void RemoveHandler(Func<T, Task> handler)
            => _handlers.Remove(handler);

        public async Task InvokeAsync(T value)
        {
            foreach (var handler in _handlers)
                await handler.Invoke(value);
        }

        private readonly List<Func<T, Task>> _handlers
            = new List<Func<T, Task>>();
    }

    public class FakeAsyncEvent<T1, T2>
    {
        public IReadOnlyCollection<Func<T1, T2, Task>> Handlers
            => _handlers;

        public void AddHandler(Func<T1, T2, Task> handler)
            => _handlers.Add(handler);

        public void RemoveHandler(Func<T1, T2, Task> handler)
            => _handlers.Remove(handler);

        public async Task InvokeAsync(T1 value1, T2 value2)
        {
            foreach (var handler in _handlers)
                await handler.Invoke(value1, value2);
        }

        private readonly List<Func<T1, T2, Task>> _handlers
            = new List<Func<T1, T2, Task>>();
    }

    public class FakeAsyncEvent<T1, T2, T3>
    {
        public IReadOnlyCollection<Func<T1, T2, T3, Task>> Handlers
            => _handlers;

        public void AddHandler(Func<T1, T2, T3, Task> handler)
            => _handlers.Add(handler);

        public void RemoveHandler(Func<T1, T2, T3, Task> handler)
            => _handlers.Remove(handler);

        public async Task InvokeAsync(T1 value1, T2 value2, T3 value3)
        {
            foreach (var handler in _handlers)
                await handler.Invoke(value1, value2, value3);
        }

        private readonly List<Func<T1, T2, T3, Task>> _handlers
            = new List<Func<T1, T2, T3, Task>>();
    }
}
