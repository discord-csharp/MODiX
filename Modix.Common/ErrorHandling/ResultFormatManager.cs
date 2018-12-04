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

        //When we resolve formatters for a ServiceResult, the type system isn't quite smart enough
        //to resolve to the most derived type, so we have to use reflection. Additionally, we can't
        //just cache instances, because IResultFormatter(s) are scoped per-request - but we do cache
        //the MethodInfo for a given (TInput, TOutput) pair
        private TOutput FindCacheAndInvokeFormatter<TOutput>(ServiceResult input)
        {
            //Get the actual, most derived type of the input
            var inputType = input.GetType();

            //Create a Type instance representing an IResultFormatter with the determined input type and provided
            //output type
            var type = typeof(IResultFormatter<,>).MakeGenericType(inputType, typeof(TOutput));

            //Try to get an instance of this most derived version from the IoC container - if it fails,
            //get the default version
            var instance = _provider.GetService(type);

            if (instance == null)
            {
                type = typeof(IResultFormatter<,>).MakeGenericType(typeof(ServiceResult), typeof(TOutput));
                instance = _provider.GetRequiredService(type);
            }

            //Finally, get the Format method for the (TInput, TOutput) pair, either from the cache or
            //from the resolved type
            var formatMethod = _formatMethodCache.GetOrAdd((inputType, typeof(TOutput)),
                (_) => type.GetMethod(nameof(IResultFormatter<ServiceResult, object>.Format)));

            //Invoke the format method on the instance of the formatter, and return the result
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
