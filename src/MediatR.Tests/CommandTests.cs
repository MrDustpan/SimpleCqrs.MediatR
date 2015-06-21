namespace MediatR.Tests
{
    using Shouldly;
    using StructureMap;
    using StructureMap.Graph;
    using System.IO;
    using System.Text;

    public class CommandTests
    {
        public class Ping : ICommand
        {
            public string Message { get; set; }
        }

        public class PingHandler : ICommandHandler<Ping>
        {
            private readonly TextWriter _writer;

            public PingHandler(TextWriter writer)
            {
                _writer = writer;
            }

            public void Handle(Ping message)
            {
                _writer.Write(message.Message + " Pong");
            }
        }

        public void Should_resolve_main_handler()
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);

            var container = new Container(cfg =>
            {
                cfg.Scan(scanner =>
                {
                    scanner.TheCallingAssembly();
                    scanner.IncludeNamespaceContainingType<Ping>();
                    scanner.WithDefaultConventions();
                    scanner.AddAllTypesOf(typeof(ICommandHandler<>));
                });
                cfg.For<TextWriter>().Use(writer);
                cfg.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
                cfg.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
                cfg.For<IMediator>().Use<Mediator>();
            });

            var mediator = container.GetInstance<IMediator>();

            mediator.Send(new Ping { Message = "Ping" });

            builder.ToString().ShouldBe("Ping Pong");
        }
    }
}