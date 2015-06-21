namespace MediatR
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a handler for a query
    /// </summary>
    /// <typeparam name="TQuery">The type of query being handled</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler</typeparam>
    public interface IQueryHandler<in TQuery, out TResponse>
        where TQuery : IQuery<TResponse>
    {
        /// <summary>
        /// Handles a query
        /// </summary>
        /// <param name="message">The query message</param>
        /// <returns>Response from the query</returns>
        TResponse Handle(TQuery message);
    }

    /// <summary>
    /// Defines an asynchronous handler for a query
    /// </summary>
    /// <typeparam name="TQuery">The type of query being handled</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler</typeparam>
    public interface IAsyncQueryHandler<in TQuery, TResponse>
        where TQuery : IAsyncQuery<TResponse>
    {
        /// <summary>
        /// Handles an asynchronous query
        /// </summary>
        /// <param name="message">The query message</param>
        /// <returns>A task representing the response from the query</returns>
        Task<TResponse> Handle(TQuery message);
    }

    /// <summary>
    /// Defines a handler for a command
    /// </summary>
    /// <typeparam name="TCommand">The type of command being handled</typeparam>
    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        /// <summary>
        /// Handles a command
        /// </summary>
        /// <param name="message">The command message</param>
        void Handle(TCommand message);
    }

    /// <summary>
    /// Defines an asynchronous handler for a command
    /// </summary>
    /// <typeparam name="TCommand">The type of command being handled</typeparam>
    public interface IAsyncCommandHandler<in TCommand>
        where TCommand : IAsyncCommand
    {
        /// <summary>
        /// Handles an asynchronous command
        /// </summary>
        /// <param name="message">The command message</param>
        Task Handle(TCommand message);
    }

    /// <summary>
    /// Defines a handler for an event
    /// </summary>
    /// <typeparam name="TEvent">The type of event being handled</typeparam>
    public interface IEventHandler<in TEvent>
        where TEvent : IEvent
    {
        /// <summary>
        /// Handles an event
        /// </summary>
        /// <param name="event">The event message</param>
        void Handle(TEvent @event);
    }

    /// <summary>
    /// Defines an asynchronous handler for an event
    /// </summary>
    /// <typeparam name="TEvent">The type of event being handled</typeparam>
    public interface IAsyncEventHandler<in TEvent>
        where TEvent : IAsyncEvent
    {
        /// <summary>
        /// Handles an asynchronous event
        /// </summary>
        /// <param name="event">The event message</param>
        /// <returns>A task representing handling the event</returns>
        Task Handle(TEvent @event);
    }
}