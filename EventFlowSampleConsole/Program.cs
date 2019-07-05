using EventFlow;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Configuration;
using EventFlow.Configuration.Serialization;
using EventFlow.Core;
using EventFlow.EventStores;
using EventFlow.Exceptions;
using EventFlow.Extensions;
using EventFlow.Queries;
using EventFlow.ReadStores;
using EventFlow.Subscribers;
using EventFlow.ValueObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventFlowSampleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var r = EventFlowOptions.New
                    .RegisterServices(serviceRegistration => {
                        serviceRegistration.Register<IJsonSerializer, MyJsonSerializer>();
                    })
                    .AddEvents(new[] {
                        typeof(SampleEvent),
                    })
                    .AddCommands(new[] {
                        typeof(SampleCommand),
                    })
                    .AddCommandHandlers(new[] {
                        typeof(SampleCommandHandler),
                    })
                    .AddSubscribers(new[] {
                        typeof(SampleSubscribeSynchronousTo),
                    })
                    .UseInMemoryReadStoreFor<SampleAReadModel>()
                    //.UseFilesEventStore(FilesEventStoreConfiguration.Create(@".\store"))
                    .UseConsoleLog()
                    .Configure((EventFlowConfiguration c) =>
                    {
                        c.IsAsynchronousSubscribersEnabled = false;
                        c.ThrowSubscriberExceptions = true;
                    })
                    .CreateResolver())
            {
                var id = SampleId.New;
                r.Resolve<ICommandBus>().Publish(new SampleCommand(id, new SampleVO("Sample ValueObject Value")));

                var ret = r.Resolve<IQueryProcessor>().Process(new ReadModelByIdQuery<SampleAReadModel>(id), CancellationToken.None);
                Console.WriteLine(ret.SampleId.Value);
                Console.WriteLine(ret.SampleVO.Value);
            }
        }
    }

    public class MyJsonSerializer : IJsonSerializer
    {
        public object Deserialize(string json, Type type)
        {
            try
            {
                return Utf8Json.JsonSerializer.NonGeneric.Deserialize(type, json);
            }catch(Exception ex)
            {
                throw ex;
            }
        }

        public T Deserialize<T>(string json) => Utf8Json.JsonSerializer.Deserialize<T>(json);
        public string Serialize(object obj, bool indented = false) => Utf8Json.JsonSerializer.ToJsonString(obj);
    }

    #region Sample
    public class SampleVO : SingleValueObject<string>
    {
        public SampleVO(string value) : base(value) { }
    }

    public class SampleId : Identity<SampleId>
    {
        public SampleId(string value) : base(value) { }
    }

    public class SampleA : AggregateRoot<SampleA, SampleId>
    {
        private SampleAState SampleAState { get; } = new SampleAState();
        public SampleVO SampleVO => SampleAState.SampleVO;

        public SampleA(SampleId id) : base(id) => Register(SampleAState);

        public IExecutionResult SetSampleVO(SampleVO _sampleVO)
        {
            Emit(new SampleEvent(_sampleVO));

            return ExecutionResult.Success();
        }
    }

    public class SampleAState : AggregateState<SampleA, SampleId, SampleAState>, IApply<SampleEvent>
    {
        public SampleVO SampleVO { get; private set; }

        public SampleAState() : base() { }

        public void Apply(SampleEvent ev) => SampleVO = ev.SampleVO;
    }

    [EventVersion("Sample", 1)]
    public class SampleEvent : AggregateEvent<SampleA, SampleId>
    {
        public SampleVO SampleVO { get; set; }// set付けないとReadModelのApplyのdomainEventのAggregateEventがnullになる.

        public SampleEvent() : base() { }// デフォルトコンストラクタないとDeserialize失敗する.

        public SampleEvent(SampleVO _sampleVO) : this() => SampleVO = _sampleVO;
    }

    public class SampleCommand : DistinctCommand<SampleA, SampleId, IExecutionResult>
    {
        public SampleVO SampleVO { get; }
        public SampleCommand(SampleId id, SampleVO _sampleVO) : base(id) => SampleVO = _sampleVO;

        protected override IEnumerable<byte[]> GetSourceIdComponents()
        {
            yield return System.Text.Encoding.UTF8.GetBytes(SampleVO.Value);
        }
    }

    public class SampleCommandHandler : CommandHandler<SampleA, SampleId, SampleCommand>
    {
        public override Task ExecuteAsync(SampleA aggregate, SampleCommand command, CancellationToken cancellationToken)
            => Task.FromResult(aggregate.SetSampleVO(command.SampleVO));
    }

    public class SampleAReadModel : IReadModel, IAmReadModelFor<SampleA, SampleId, SampleEvent>
    {
        public SampleId SampleId { get; private set; }
        public SampleVO SampleVO { get; private set; }

        public void Apply(IReadModelContext context, IDomainEvent<SampleA, SampleId, SampleEvent> domainEvent)
        {
            SampleId = domainEvent.AggregateIdentity;
            SampleVO = domainEvent.AggregateEvent.SampleVO;
        }
    }

    public class SampleSubscribeSynchronousTo : ISubscribeSynchronousTo<SampleA, SampleId, SampleEvent>
    {
        public Task HandleAsync(IDomainEvent<SampleA, SampleId, SampleEvent> domainEvent, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
    #endregion Sample
}
