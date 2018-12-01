using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Modix.Common.ErrorHandling
{
    /// <summary>
    /// Handles creating instances of <see cref="IResultFormatter{TInput, TOutput}"/> and using them to format ServiceResults
    /// </summary>
    public interface IResultFormatManager
    {
        /// <summary>
        /// Formats the given <typeparamref name="TInput"/> into a <typeparamref name="TOutput"/>
        /// </summary>
        /// <typeparam name="TInput">The type of the input</typeparam>
        /// <typeparam name="TOutput">The type that the <typeparamref name="TInput"/> instance will be formatted into</typeparam>
        /// <param name="result">The instance of <typeparamref name="TInput"/> to be formatted</param>
        /// <returns>An instance of <typeparamref name="TOutput"/> based on the <typeparamref name="TInput"/></returns>
        TOutput Format<TInput, TOutput>(TInput result) where TInput : ServiceResult;
    }

    /// <inheritdoc />
    public class ResultFormatManager : IResultFormatManager
    {
        private readonly IServiceProvider _provider;

        private static readonly ConcurrentDictionary<(Type, Type), MethodInfo> _formatMethodCache
            = new ConcurrentDictionary<(Type, Type), MethodInfo>();

        public ResultFormatManager(IServiceProvider provider)
        {
            _provider = provider;
        }

        private TOutput FindCacheAndInvokeFormatter<TOutput>(ServiceResult input)
        {
            var inputType = input.GetType();

            var type = typeof(IResultFormatter<,>).MakeGenericType(inputType, typeof(TOutput));

            var instance = _provider.GetService(type);

            if (instance == null)
            {
                type = typeof(IResultFormatter<,>).MakeGenericType(typeof(ServiceResult), typeof(TOutput));
                instance = _provider.GetRequiredService(type);
            }

            var formatMethod = _formatMethodCache.GetOrAdd((inputType, typeof(TOutput)),
                (_) => type.GetMethod(nameof(IResultFormatter<ServiceResult, object>.Format)));

            return (TOutput)formatMethod.Invoke(instance, new object[] { input });
        }

        /// <inheritdoc />
        public TOutput Format<TInput, TOutput>(TInput result) where TInput : ServiceResult
        {
            var found = FindCacheAndInvokeFormatter<TOutput>(result);
            return found;
        }
    }
}
