using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Core;
using EventFlow.EventStores;
using EventFlow.Queries;
using EventFlow.ReadStores;
using EventFlow.ValueObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.利用者
{
    public class 氏名 : ValueObject
    {
        public string Value { get; }
        public 氏名(string value) => Value = value;
        public static 氏名 Create(string value) => new 氏名(value);
    }

    public class 利用者のID : Identity<利用者のID>
    {
        public 利用者のID(string value) : base(value) { }
    }

    public class 利用者 : AggregateRoot<利用者, 利用者のID>, IEmit<利用者を登録した>
    {
        public 氏名 氏名 { get; set; }
        public 利用者(利用者のID id) : base(id) { }

        public void 利用者を登録する(氏名 _氏名) => Emit(new 利用者を登録した(_氏名));
        public void Apply(利用者を登録した ev) => 氏名 = ev.氏名;
    }

    [EventVersion("利用者を登録した", 1)]
    public class 利用者を登録した : AggregateEvent<利用者, 利用者のID>
    {
        public 氏名 氏名 { get; set; }

        public 利用者を登録した() : base() { }

        public 利用者を登録した(氏名 _氏名) : this() => 氏名 = _氏名;
    }

    public class 利用者を登録するCommand : Command<利用者, 利用者のID, IExecutionResult>
    {
        public 氏名 氏名 { get; }
        public 利用者を登録するCommand(利用者のID id, 氏名 _氏名) : base(id) => 氏名 = _氏名;
    }

    public class 利用者を登録するCommandHandler : CommandHandler<利用者, 利用者のID, IExecutionResult, 利用者を登録するCommand>
    {
        public 利用者を登録するCommandHandler() : base() { }
        public override Task<IExecutionResult> ExecuteCommandAsync(利用者 aggregate, 利用者を登録するCommand command, CancellationToken cancellationToken)
        {
            aggregate.利用者を登録する(command.氏名);
            return Task.FromResult(ExecutionResult.Success());
        }
    }

    public class 利用者ReadModel : IReadModel, IAmReadModelFor<利用者, 利用者のID, 利用者を登録した>
    {
        public 利用者のID 利用者のID { get; private set; }
        public 氏名 氏名 { get; private set; }

        public void Apply(IReadModelContext context, IDomainEvent<利用者, 利用者のID, 利用者を登録した> domainEvent)
        {
            利用者のID = domainEvent.AggregateIdentity;
            氏名 = domainEvent.AggregateEvent.氏名;
        }
    }
}
