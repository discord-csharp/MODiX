using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Common.ErrorHandling
{
    /// <summary>
    /// Represents the result of a service call with no return value
    /// </summary>
    public class ServiceResult
    {
        private static readonly ServiceResult _successfulResult = new ServiceResult { IsSuccess = true };

        public bool IsSuccess { get; protected set; }
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
        /// Returns the first ServiceResult if it failed, otherwise, returns a new successful ServiceResult<<typeparamref name="T"/>> for the second
        /// </summary>
        /// <remarks>Useful for returning a generic ServiceResult when you have a non-generic condition</remarks>
        /// <typeparam name="T">The return type of the ServiceResult if the first succeeded</typeparam>
        /// <param name="condition">The conditional ServiceResult - if failed, will be returned</param>
        /// <param name="result">The instance that will be assigned to the returned ServiceResult<typeparamref name="T"/></param>
        public static ServiceResult<T> ShortCircuit<T>(ServiceResult condition, T result)
        {
            if (condition.IsFailure)
            {
                return (ServiceResult<T>)condition;
            }

            return new ServiceResult<T>() { IsSuccess = true, Result = result };
        }
    }

    /// <summary>
    /// Represents the result of a service call with a return value
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Result"/></typeparam>
    public class ServiceResult<T> : ServiceResult
    {
        internal protected ServiceResult() { }
        public T Result { get; internal protected set; }

        public static new ServiceResult<T> FromError(string error)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Error = error
            };
        }

        /// <summary>
        /// Returns the passed ServiceResult if it failed, otherwise, returns this one
        /// </summary>
        /// <remarks>Useful for returning a generic ServiceResult when you have a non-generic condition</remarks>
        /// <param name="condition">The conditional ServiceResult - if failed, will be returned</param>
        public ServiceResult<T> ShortCircuit(ServiceResult condition)
        {
            if (condition.IsFailure)
            {
                return (ServiceResult<T>)condition;
            }

            return this;
        }
    }
}
