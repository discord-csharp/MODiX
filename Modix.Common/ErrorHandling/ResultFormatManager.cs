using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private const string ConditionPropertyName = nameof(ConditionalServiceResult<object, ServiceResult<object>>.Condition);
        private const string FormatMethodName = nameof(IResultFormatter<ServiceResult, object>.Format);

        private static readonly ConcurrentDictionary<(Type, Type), Type> _formatterTypeCache
            = new ConcurrentDictionary<(Type, Type), Type>();

        private static readonly ConcurrentDictionary<(Type, Type), MethodInfo> _formatMethodCache
            = new ConcurrentDictionary<(Type, Type), MethodInfo>();

        private readonly IServiceProvider _provider;

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
            var typeKey = (input: inputType, output: typeof(TOutput));

            object instance = null;

            //Special case for conditional results, we need to get the formatter for the type of the conditional within
            if (inputType.IsGenericType && inputType.GetGenericTypeDefinition() == typeof(ConditionalServiceResult<,>))
            {
                //Reassign input to the given condition, and the inputType to the type of that condition so it resolves correctly
                input = typeKey.input.GetProperty(ConditionPropertyName).GetValue(input) as ServiceResult;
                inputType = input.GetType();
            }

            if (_formatterTypeCache.ContainsKey(typeKey) == false)
            {
                //Create a Type instance representing an IResultFormatter with the determined input type and provided
                //output type
                var type = typeof(IResultFormatter<,>).MakeGenericType(inputType, typeof(TOutput));

                //Try to get an instance of this most derived version from the IoC container
                instance = _provider.GetService(type);

                //If we can't find a formatter for the derived ServiceResult type, try to find one for the
                //base type (kind of a catch-all)
                if (instance == null)
                {
                    type = typeof(IResultFormatter<ServiceResult, TOutput>);
                }

                //Add the type and its format method to the cache - we can't add the actual instance
                //because it's instantiated per-scope
                _formatterTypeCache.TryAdd(typeKey, type);
                _formatMethodCache.TryAdd(typeKey, type.GetMethod(FormatMethodName));
            }

            instance = _provider.GetRequiredService(_formatterTypeCache[typeKey]);

            //Get the Format method for the (TInput, TOutput) pair from the cache
            var formatMethod = _formatMethodCache[typeKey];

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
