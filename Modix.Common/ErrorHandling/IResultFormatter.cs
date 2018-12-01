using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Common.ErrorHandling
{
    /// <summary>
    /// Represents a class that can format a service result
    /// </summary>
    /// <typeparam name="TInput">The input to be formatted, which derives from <see cref="ServiceResult"/></typeparam>
    /// <typeparam name="TOutput">The format output type</typeparam>
    public interface IResultFormatter<TInput, TOutput> where TInput : ServiceResult
    {
        /// <summary>
        /// Formats the given <typeparamref name="TInput"/> into a <typeparamref name="TOutput"/>
        /// </summary>
        /// <param name="result">The <typeparamref name="TInput"/> to be formatted</param>
        /// <returns>An instance of <typeparamref name="TOutput"/></returns>
        TOutput Format(TInput result);
    }
}
