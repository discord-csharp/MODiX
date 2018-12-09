using System;
using System.Threading.Tasks;

namespace Discord
{
    internal static class AsyncEventExtensions
    {
        public static async Task InvokeAsync(this Func<Task> @event)
        {
            foreach (Func<Task> handler in @event.GetInvocationList())
                await handler.Invoke();
        }

        public static async Task InvokeAsync<T>(this Func<T, Task> @event, T arg)
        {
            foreach (Func<T, Task> handler in @event.GetInvocationList())
                await handler.Invoke(arg);
        }

        public static async Task InvokeAsync<T1, T2>(this Func<T1, T2, Task> @event, T1 arg1, T2 arg2)
        {
            foreach (Func<T1, T2, Task> handler in @event.GetInvocationList())
                await handler.Invoke(arg1, arg2);
        }

        public static async Task InvokeAsync<T1, T2, T3>(this Func<T1, T2, T3, Task> @event, T1 arg1, T2 arg2, T3 arg3)
        {
            foreach (Func<T1, T2, T3, Task> handler in @event.GetInvocationList())
                await handler.Invoke(arg1, arg2, arg3);
        }
    }
}
