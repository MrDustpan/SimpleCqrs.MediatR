namespace MediatR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a mediator to encapsulate sending commands, query/response and publishing events
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Send a command to a single handler
        /// </summary>
        /// <param name="command">Command object</param>
        void Send(ICommand command);

        /// <summary>
        /// Asynchronously send a command to a single handler 
        /// </summary>
        /// <param name="command">Command object</param>
        Task SendAsync(IAsyncCommand command);

        /// <summary>
        /// Send a query to a single handler
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="query">Query object</param>
        /// <returns>Response</returns>
        TResponse Send<TResponse>(IQuery<TResponse> query);

        /// <summary>
        /// Asynchronously send a query to a single handler 
        /// </summary>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="query">Query object</param>
        /// <returns>A task that represents the send operation. The task result contains the handler response</returns>
        Task<TResponse> SendAsync<TResponse>(IAsyncQuery<TResponse> query);

        /// <summary>
        /// Send an event to multiple handlers
        /// </summary>
        /// <param name="event">Event object</param>
        void Publish(IEvent @event);

        /// <summary>
        /// Asynchronously send an event to multiple handlers
        /// </summary>
        /// <param name="event">Event object</param>
        /// <returns>A task that represents the publish operation.</returns>
        Task PublishAsync(IAsyncEvent @event);
    }

    /// <summary>
    /// Factory method for creating single instances. Used to build instances of <see cref="IQueryHandler{TQuery,TResponse}"/> and <see cref="IAsyncQueryHandler{TQuery,TResponse}"/>
    /// </summary>
    /// <param name="serviceType">Type of service to resolve</param>
    /// <returns>An instance of type <paramref name="serviceType" /></returns>
    public delegate object SingleInstanceFactory(Type serviceType);

    /// <summary>
    /// Factory method for creating multiple instances. Used to build instances of <see cref="IEventHandler{TEvent}"/> and <see cref="IAsyncEventHandler{TEvent}"/>
    /// </summary>
    /// <param name="serviceType">Type of service to resolve</param>
    /// <returns>An enumerable of instances of type <paramref name="serviceType" /></returns>
    public delegate IEnumerable<object> MultiInstanceFactory(Type serviceType);

    /// <summary>
    /// Default mediator implementation relying on Common Service Locator for resolving handlers
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly SingleInstanceFactory _singleInstanceFactory;
        private readonly MultiInstanceFactory _multiInstanceFactory;

        public Mediator(SingleInstanceFactory singleInstanceFactory, MultiInstanceFactory multiInstanceFactory)
        {
            _singleInstanceFactory = singleInstanceFactory;
            _multiInstanceFactory = multiInstanceFactory;
        }

        public void Send(ICommand command)
        {
            var defaultHandler = GetCommandHandler(command);

            defaultHandler.Handle(command);
        }

        public async Task SendAsync(IAsyncCommand command)
        {
            var defaultHandler = GetCommandHandler(command);

            await defaultHandler.Handle(command);
        }

        public TResponse Send<TResponse>(IQuery<TResponse> query)
        {
            var defaultHandler = GetQueryHandler(query);

            TResponse result = defaultHandler.Handle(query);

            return result;
        }

        public async Task<TResponse> SendAsync<TResponse>(IAsyncQuery<TResponse> query)
        {
            var defaultHandler = GetQueryHandler(query);

            TResponse result = await defaultHandler.Handle(query);

            return result;
        }

        public void Publish(IEvent @event)
        {
            var eventHandlers = GetEventHandlers(@event);

            foreach (var handler in eventHandlers)
            {
                handler.Handle(@event);
            }
        }

        public async Task PublishAsync(IAsyncEvent @event)
        {
            var eventHandlers = GetAsyncEventHandlers(@event);

            foreach (var handler in eventHandlers)
            {
                await handler.Handle(@event);
            }
        }

        private static InvalidOperationException BuildException(object message, Exception inner = null)
        {
            return new InvalidOperationException("Handler was not found for query of type " + message.GetType() + ".\r\nContainer or service locator not configured properly or handlers not registered with your container.", inner);
        }

        private QueryHandler<TResponse> GetQueryHandler<TResponse>(IQuery<TResponse> query)
        {
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
            var wrapperType = typeof(QueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
            object handler;
            try
            {
                handler = _singleInstanceFactory(handlerType);

                if (handler == null)
                    throw BuildException(query);
            }
            catch (Exception e)
            {
                throw BuildException(query, e);
            }
            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (QueryHandler<TResponse>)wrapperHandler;
        }

        private AsyncQueryHandler<TResponse> GetQueryHandler<TResponse>(IAsyncQuery<TResponse> query)
        {
            var handlerType = typeof(IAsyncQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
            var wrapperType = typeof(AsyncQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
            object handler;
            try
            {
                handler = _singleInstanceFactory(handlerType);

                if (handler == null)
                    throw BuildException(query);
            }
            catch (Exception e)
            {
                throw BuildException(query, e);
            }

            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (AsyncQueryHandler<TResponse>)wrapperHandler;
        }

        private CommandHandler GetCommandHandler(ICommand command)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
            var wrapperType = typeof(CommandHandler<>).MakeGenericType(command.GetType());
            object handler;
            try
            {
                handler = _singleInstanceFactory(handlerType);

                if (handler == null)
                    throw BuildException(command);
            }
            catch (Exception e)
            {
                throw BuildException(command, e);
            }
            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (CommandHandler)wrapperHandler;
        }

        private AsyncCommandHandler GetCommandHandler(IAsyncCommand command)
        {
            var handlerType = typeof(IAsyncCommandHandler<>).MakeGenericType(command.GetType());
            var wrapperType = typeof(AsyncCommandHandler<>).MakeGenericType(command.GetType());
            object handler;
            try
            {
                handler = _singleInstanceFactory(handlerType);

                if (handler == null)
                    throw BuildException(command);
            }
            catch (Exception e)
            {
                throw BuildException(command, e);
            }

            var wrapperHandler = Activator.CreateInstance(wrapperType, handler);
            return (AsyncCommandHandler)wrapperHandler;
        }

        private IEnumerable<EventHandler> GetEventHandlers(IEvent @event)
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
            var wrapperType = typeof(EventHandler<>).MakeGenericType(@event.GetType());

            var handlers = _multiInstanceFactory(handlerType);

            return handlers.Select(handler => (EventHandler)Activator.CreateInstance(wrapperType, handler)).ToList();
        }

        private IEnumerable<AsyncEventHandler> GetAsyncEventHandlers(IAsyncEvent @event)
        {
            var handlerType = typeof(IAsyncEventHandler<>).MakeGenericType(@event.GetType());
            var wrapperType = typeof(AsyncEventHandler<>).MakeGenericType(@event.GetType());

            var handlers = _multiInstanceFactory(handlerType);

            return handlers.Select(handler => (AsyncEventHandler)Activator.CreateInstance(wrapperType, handler)).ToList();
        }

        private abstract class CommandHandler
        {
            public abstract void Handle(ICommand message);
        }

        private class CommandHandler<TCommand> : CommandHandler where TCommand : ICommand
        {
            private readonly ICommandHandler<TCommand> _inner;

            public CommandHandler(ICommandHandler<TCommand> inner)
            {
                _inner = inner;
            }

            public override void Handle(ICommand message)
            {
                _inner.Handle((TCommand)message);
            }
        }

        private abstract class QueryHandler<TResult>
        {
            public abstract TResult Handle(IQuery<TResult> message);
        }

        private class QueryHandler<TQuery, TResult> : QueryHandler<TResult> where TQuery : IQuery<TResult>
        {
            private readonly IQueryHandler<TQuery, TResult> _inner;

            public QueryHandler(IQueryHandler<TQuery, TResult> inner)
            {
                _inner = inner;
            }

            public override TResult Handle(IQuery<TResult> message)
            {
                return _inner.Handle((TQuery)message);
            }
        }

        private abstract class EventHandler
        {
            public abstract void Handle(IEvent message);
        }

        private class EventHandler<TEvent> : EventHandler where TEvent : IEvent
        {
            private readonly IEventHandler<TEvent> _inner;

            public EventHandler(IEventHandler<TEvent> inner)
            {
                _inner = inner;
            }

            public override void Handle(IEvent message)
            {
                _inner.Handle((TEvent)message);
            }
        }

        private abstract class AsyncCommandHandler
        {
            public abstract Task Handle(IAsyncCommand message);
        }

        private class AsyncCommandHandler<TCommand> : AsyncCommandHandler
            where TCommand : IAsyncCommand
        {
            private readonly IAsyncCommandHandler<TCommand> _inner;

            public AsyncCommandHandler(IAsyncCommandHandler<TCommand> inner)
            {
                _inner = inner;
            }

            public override Task Handle(IAsyncCommand message)
            {
                return _inner.Handle((TCommand)message);
            }
        }

        private abstract class AsyncQueryHandler<TResult>
        {
            public abstract Task<TResult> Handle(IAsyncQuery<TResult> message);
        }

        private class AsyncQueryHandler<TQuery, TResult> : AsyncQueryHandler<TResult>
            where TQuery : IAsyncQuery<TResult>
        {
            private readonly IAsyncQueryHandler<TQuery, TResult> _inner;

            public AsyncQueryHandler(IAsyncQueryHandler<TQuery, TResult> inner)
            {
                _inner = inner;
            }

            public override Task<TResult> Handle(IAsyncQuery<TResult> message)
            {
                return _inner.Handle((TQuery)message);
            }
        }

        private abstract class AsyncEventHandler
        {
            public abstract Task Handle(IAsyncEvent message);
        }

        private class AsyncEventHandler<TEvent> : AsyncEventHandler
            where TEvent : IAsyncEvent
        {
            private readonly IAsyncEventHandler<TEvent> _inner;

            public AsyncEventHandler(IAsyncEventHandler<TEvent> inner)
            {
                _inner = inner;
            }

            public override Task Handle(IAsyncEvent message)
            {
                return _inner.Handle((TEvent)message);
            }
        }
    }
}