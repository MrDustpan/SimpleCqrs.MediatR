namespace MediatR.Tests
{
    using System.Threading.Tasks;
    using Shouldly;
    using StructureMap;
    using StructureMap.Graph;

    public class AsyncQueryTests
    {

        public class Ping : IAsyncQuery<Pong>
        {
            public string Message { get; set; }
        }

        public class Pong
        {
            public string Message { get; set; }
        }

        public class PingHandler : IAsyncQueryHandler<Ping, Pong>
        {
            public async Task<Pong> Handle(Ping message)
            {
                return await Task.Factory.StartNew(() => new Pong {Message = message.Message + " Pong"});
            }
        }

        public void Should_resolve_main_handler()
        {
            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.TheCallingAssembly();
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof (IAsyncQueryHandler<,>));
                });
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            var response = mediator.SendAsync(new Ping { Message = "Ping" });

            Task.WaitAll(response);

            response.Result.Message.ShouldBe("Ping Pong");
        }
    }
}