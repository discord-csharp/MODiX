using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Modix.Common.ErrorHandling
{
    /// <summary>
    /// Represents the result of a service call with no return value
    /// </summary>
    public class ServiceResult
    {
        private static readonly ServiceResult _successfulResult = new ServiceResult { IsSuccess = true };

        public virtual bool IsSuccess { get; protected set; }
        public virtual bool IsFailure => !IsSuccess;
        public string Error { get; protected set; }

        protected ServiceResult() { }

        /// <summary>
        /// Returns the ServiceResult as an exception for throwing
        /// </summary>
        public virtual Exception AsException()
        {
            return new InvalidOperationException(Error);
        }

        public override string ToString()
            => IsSuccess ? "Success" : $"Failure {Error}";

        /// <summary>
        /// Returns a successful ServiceResult
        /// </summary>
        public static ServiceResult FromSuccess()
            => _successfulResult;

        /// <summary>
        /// Returns a failed ServiceResult with the <see cref="Error"/> populated from the given error
        /// </summary>
        /// <param name="error">The error for the ServiceResult</param>
        public static ServiceResult FromError(string error)
        {
            return new ServiceResult
            {
                IsSuccess = false,
                Error = error
            };
        }

        /// <summary>
        /// Returns a failed ServiceResult with the <see cref="Error"/> populated from an exception
        /// </summary>
        /// <param name="ex">The exception to populate the <see cref="Error"/> with</param>
        public static ServiceResult FromException(Exception ex)
        {
            return new ServiceResult
            {
                IsSuccess = false,
                Error = ex.ToString()
            };
        }

        /// <summary>
        /// Returns a (default) successful ServiceResult<<typeparamref name="T"/>> from the given result
        /// </summary>
        /// <param name="result">The <see cref="ServiceResult{T}.Result"/> of the returned <see cref="ServiceResult{T}"/></param>
        /// <param name="success">Whether or not the <see cref="ServiceResult{T}"/> should be successful</param>
        public static ServiceResult<T> FromResult<T>(T result, bool success = true)
        {
            return new ServiceResult<T>
            {
                IsSuccess = success,
                Result = result
            };
        }

        /// <summary>
        /// Returns a <see cref="ConditionalServiceResult{TData, TGeneric}"/> which will be successful if this result is
        /// </summary>
        /// <typeparam name="TServiceResult">The type of the conditional service result</typeparam>
        /// <typeparam name="TData">The type of the data to be returned in the result</typeparam>
        /// <param name="result">A task that, if the condition is successful, will be awaited and accessible via <see cref="ConditionalServiceResult{TData, TGeneric}.Result"/></param>
        public async Task<ConditionalServiceResult<TData, ServiceResult<TData>>> ShortCircuitAsync<TData>(Task<TData> result)
        {
            var ret = new ServiceResult<TData>();

            if (IsSuccess)
            {
                ret = FromResult(await result);
            }

            return new ConditionalServiceResult<TData, ServiceResult<TData>>(ret, this);
        }
    }

    public class ConditionalServiceResult<TData, TGeneric> : ServiceResult<TData>
        where TGeneric : ServiceResult<TData>
    {
        public TGeneric GenericResult { get; private set; }
        public ServiceResult Condition { get; private set; }

        public override bool IsSuccess => Condition.IsSuccess ? true : false;
        public override TData Result => GenericResult.Result;

        public ConditionalServiceResult(TGeneric genericResult, ServiceResult baseResult)
        {
            GenericResult = genericResult;
            Condition = baseResult;
        }
    }

    /// <summary>
    /// Represents the result of a service call with a return value
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Result"/></typeparam>
    public class ServiceResult<T> : ServiceResult
    {
        internal protected ServiceResult() { }
        public virtual T Result { get; internal protected set; }

        public static new ServiceResult<T> FromError(string error)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Error = error
            };
        }
    }
}
