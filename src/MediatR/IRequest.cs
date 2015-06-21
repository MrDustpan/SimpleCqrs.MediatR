namespace MediatR
{
    /// <summary>
    /// Marker interface to represent a query with a response
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface IQuery<out TResponse> { }

    /// <summary>
    /// Marker interface to represent an asynchronous query with a response
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    public interface IAsyncQuery<out TResponse> { }

    /// <summary>
    /// Marker interface to represent a command
    /// </summary>
    public interface ICommand { }

    /// <summary>
    /// Marker interface to represent an asynchronous command
    /// </summary>
    public interface IAsyncCommand { }

    /// <summary>
    /// Marker interface to represent an event
    /// </summary>
    public interface IEvent { }

    /// <summary>
    /// Marker interface to represent an asynchronous event
    /// </summary>
    public interface IAsyncEvent { }
}