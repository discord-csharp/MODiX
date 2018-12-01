namespace Modix.Common.Messaging
{
    /// <summary>
    /// Identifies a class that describes an application-wide request
    /// operation, which is dispatched by an <see cref="IMediator"/>
    /// and handled by an <see cref="IRequestHandler{TRequest, TResponse}"/>.
    /// </summary>
    /// <typeparam name="TResponse">The type of object returned as a response.</typeparam>
    public interface IRequest<TResponse> { }
}
