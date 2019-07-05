using Domain.利用者;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Core;
using EventFlow.EventStores;
using EventFlow.ReadStores;
using EventFlow.Snapshots;
using EventFlow.ValueObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.本
{
    [JsonConverter(typeof(SingleValueObjectConverter))]
    public class 貸出期間自 : SingleValueObject<DateTime>
    {
        public 貸出期間自(DateTime value) : base(value) { }
        public static 貸出期間自 今日() => new 貸出期間自(DateTime.Today); // プロパティにするとシリアライズの対象になる.
        public string 西暦() => Value.ToLongDateString();
    }

    [JsonConverter(typeof(SingleValueObjectConverter))]
    public class 貸出期間至 : SingleValueObject<DateTime>
    {
        public 貸出期間至(DateTime value) : base(value) { }
        public static 貸出期間至 二週間後() => new 貸出期間至(DateTime.Today.AddDays(14));
        public string 西暦() => Value.ToLongDateString();
    }

    public class 貸出期間 : ValueObject
    {
        public 貸出期間自 貸出期間自 { get; set; } // Converterを用意しないとset取れない(取ると値はいらない).
        public 貸出期間至 貸出期間至 { get; set; }

        public 貸出期間() : base() { }
        public 貸出期間(貸出期間自 _貸出期間自, 貸出期間至 _貸出期間至) : this()
        {
            貸出期間自 = _貸出期間自 ?? throw new ArgumentNullException(nameof(_貸出期間自));
            貸出期間至 = _貸出期間至 ?? throw new ArgumentNullException(nameof(_貸出期間至));
        }
        public static 貸出期間 今日から２週間 => new 貸出期間(貸出期間自.今日(), 貸出期間至.二週間後());
        public string 画面表示() => $"{貸出期間自.西暦()}～{貸出期間至.西暦()}";
    }

    [JsonConverter(typeof(SingleValueObjectConverter))]
    public class 本のタイトル : SingleValueObject<string>
    {
        public 本のタイトル(string value) : base(value) { }
        public static 本のタイトル Create(string value) => new 本のタイトル(value);
    }

    [JsonConverter(typeof(SingleValueObjectConverter))]
    public class 本のID : Identity<本のID>
    {
        public 本のID(string value) : base(value) { }
    }

    public class 本 : AggregateRoot<本, 本のID>
    {
        private 本を登録した状態 本を登録した状態 { get; } = new 本を登録した状態();
        private 本を借りた状態 本を借りた状態 { get; } = new 本を借りた状態();
        private 本を返した状態 本を返した状態 { get; } = new 本を返した状態();
        private 本を破棄した状態 本を破棄した状態 { get; } = new 本を破棄した状態();

        public 本のタイトル 本のタイトル { get; set; }
        public 貸出期間 貸出期間 { get; set; }
        public 利用者のID 利用者のID { get; set; }
        public 本(本のID id) : base(id)
        {
            Register(本を登録した状態);
            Register(本を借りた状態);
            Register(本を返した状態);
            Register(本を破棄した状態);
        }

        public void 本を登録する(本のタイトル _本のタイトル) => Emit(new 本を登録した(_本のタイトル));

        public void 本を借りる(貸出期間 _貸出期間, 利用者のID _利用者のID) => Emit(new 本を借りた(_貸出期間, _利用者のID));

        public void 本を返す() => Emit(new 本を返した());

        public void 本を破棄する() => Emit(new 本を破棄した());
    }

    [EventVersion("本を登録した", 1)]
    public class 本を登録した : AggregateEvent<本, 本のID>
    {
        public 本のタイトル 本のタイトル  { get; set; }
        public 本を登録した() : base() { }
        public 本を登録した(本のタイトル _本のタイトル) : this() => 本のタイトル = _本のタイトル;
    }

    public class 本を登録した状態 : AggregateState<本, 本のID, 本を登録した状態>, IApply<本を登録した>
    {
        public 本のタイトル 本のタイトル { get; set; }

        public 本を登録した状態(): base() { }

        public void Apply(本を登録した aggregateEvent) => 本のタイトル = aggregateEvent.本のタイトル;
    }

    public class 本を登録するCommand : Command<本, 本のID>
    {
        public 本のタイトル 本のタイトル { get; }
        public 本を登録するCommand(本のID id, 本のタイトル _本のタイトル) : base(id) => 本のタイトル = _本のタイトル;
    }

    public class 本を登録するCommandHandler : CommandHandler<本, 本のID, IExecutionResult, 本を登録するCommand>
    {
        public 本を登録するCommandHandler() : base() { }
        public override Task<IExecutionResult> ExecuteCommandAsync(本 aggregate, 本を登録するCommand command, CancellationToken cancellationToken)
        {
            aggregate.本を登録する(command.本のタイトル);
            return Task.FromResult(ExecutionResult.Success());
        }
    }

    [EventVersion("本を借りた", 1)]
    public class 本を借りた : AggregateEvent<本, 本のID>
    {
        public 貸出期間 貸出期間 { get; set; }
        public 利用者のID 利用者のID { get; set; }

        public 本を借りた() : base() { }
        public 本を借りた(貸出期間 _貸出期間, 利用者のID _利用者のID) : this()
        {
            貸出期間 = _貸出期間;
            利用者のID = _利用者のID;
        }
    }

    public class 本を借りた状態 : AggregateState<本, 本のID, 本を借りた状態>, IApply<本を借りた>
    {
        public 貸出期間 貸出期間 { get; set; }
        public 利用者のID 利用者のID { get; set; }

        public 本を借りた状態() : base() { }

        public void Apply(本を借りた aggregateEvent)
        {
            貸出期間 = aggregateEvent.貸出期間;
            利用者のID = aggregateEvent.利用者のID;
        }
    }

    public class 本を借りるCommand : Command<本, 本のID>
    {
        public 貸出期間 貸出期間 { get; }
        public 利用者のID 利用者のID { get; }
        public 本を借りるCommand(本のID id, 貸出期間 _貸出期間, 利用者のID _利用者のID) : base(id)
        {
            貸出期間 = _貸出期間;
            利用者のID = _利用者のID;
        }
    }

    public class 本を借りるCommandHandler : CommandHandler<本, 本のID, IExecutionResult, 本を借りるCommand>
    {
        public 本を借りるCommandHandler() : base() { }
        public override Task<IExecutionResult> ExecuteCommandAsync(本 aggregate, 本を借りるCommand command, CancellationToken cancellationToken)
        {
            aggregate.本を借りる(command.貸出期間, command.利用者のID);
            return Task.FromResult(ExecutionResult.Success());
        }
    }

    [EventVersion("本を返した", 1)]
    public class 本を返した : AggregateEvent<本, 本のID> { }

    public class 本を返した状態 : AggregateState<本, 本のID, 本を返した状態>, IApply<本を返した>
    {
        public 貸出期間 貸出期間 { get; set; }
        public 利用者のID 利用者のID { get; set; }

        public 本を返した状態() : base() { }

        public void Apply(本を返した aggregateEvent)
        {
            貸出期間 = null;
            利用者のID = null;
        }
    }

    public class 本を返すCommand : Command<本, 本のID>
    {
        public 本を返すCommand(本のID id) : base(id) { }
    }

    public class 本を返すCommandHandler : CommandHandler<本, 本のID, IExecutionResult, 本を返すCommand>
    {
        public 本を返すCommandHandler() : base() { }
        public override Task<IExecutionResult> ExecuteCommandAsync(本 aggregate, 本を返すCommand command, CancellationToken cancellationToken)
        {
            aggregate.本を返す();
            return Task.FromResult(ExecutionResult.Success());
        }
    }

    [EventVersion("本を破棄した", 1)]
    public class 本を破棄した : AggregateEvent<本, 本のID> { }

    public class 本を破棄した状態 : AggregateState<本, 本のID, 本を破棄した状態>, IApply<本を破棄した>
    {
        public 本を破棄した状態() : base() { }

        public void Apply(本を破棄した aggregateEvent)
        {
        }
    }

    public class 本を破棄するCommand : Command<本, 本のID>
    {
        public 本を破棄するCommand(本のID id) : base(id) { }
    }

    public class 本を破棄するCommandHandler : CommandHandler<本, 本のID, IExecutionResult, 本を破棄するCommand>
    {
        public 本を破棄するCommandHandler() : base() { }
        public override Task<IExecutionResult> ExecuteCommandAsync(本 aggregate, 本を破棄するCommand command, CancellationToken cancellationToken)
        {
            aggregate.本を破棄する();
            return Task.FromResult(ExecutionResult.Success());
        }
    }
    public class 本ReadModel : IReadModel
        , IAmReadModelFor<本, 本のID, 本を登録した>
        , IAmReadModelFor<本, 本のID, 本を借りた>
        , IAmReadModelFor<本, 本のID, 本を返した>
        , IAmReadModelFor<本, 本のID, 本を破棄した>
    {
        public 本のID 本のID { get; set; }
        public 本のタイトル 本のタイトル { get; set; }

        public 利用者のID 利用者のID { get; set; }
        public 貸出期間 貸出期間 { get; set; }

        public void Apply(IReadModelContext context, IDomainEvent<本, 本のID, 本を登録した> domainEvent)
        {
            本のID = domainEvent.AggregateIdentity;
            本のタイトル = domainEvent.AggregateEvent.本のタイトル;
        }

        public void Apply(IReadModelContext context, IDomainEvent<本, 本のID, 本を借りた> domainEvent)
        {
            本のID = domainEvent.AggregateIdentity;
            利用者のID = domainEvent.AggregateEvent.利用者のID;
            貸出期間 = domainEvent.AggregateEvent.貸出期間;
        }

        public void Apply(IReadModelContext context, IDomainEvent<本, 本のID, 本を返した> domainEvent)
        {
            本のID = domainEvent.AggregateIdentity;
            利用者のID = null;
            貸出期間 = null;
        }

        public void Apply(IReadModelContext context, IDomainEvent<本, 本のID, 本を破棄した> domainEvent)
        {
            context.MarkForDeletion();
        }
    }
}
