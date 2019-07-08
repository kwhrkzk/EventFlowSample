using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Core;
using EventFlow.EventStores;
using EventFlow.Queries;
using EventFlow.ReadStores;
using EventFlow.Snapshots;
using EventFlow.Snapshots.Strategies;
using EventFlow.ValueObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.利用者
{
    [JsonConverter(typeof(SingleValueObjectConverter))]
    public class 氏名 : SingleValueObject<string>
    {
        public 氏名(string value) : base(value) { }
        public static 氏名 Create(string value) => new 氏名(value);
    }

    [JsonConverter(typeof(SingleValueObjectConverter))]
    public class 利用者のID : Identity<利用者のID>
    {
        public 利用者のID(string value) : base(value) { }
    }

    public class 利用者 : SnapshotAggregateRoot<利用者, 利用者のID, 利用者Snapshot>, IEmit<利用者を登録した>
    {
        public 氏名 氏名 { get; set; }
        public 利用者(利用者のID id) : base(id, SnapshotEveryFewVersionsStrategy.Default) { }

        public void 利用者を登録する(氏名 _氏名) => Emit(new 利用者を登録した(_氏名));
        public void Apply(利用者を登録した ev) => 氏名 = ev.氏名;

        protected override Task<利用者Snapshot> CreateSnapshotAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new 利用者Snapshot() { 利用者のID = Id, 氏名 = 氏名 });
        }

        protected override Task LoadSnapshotAsync(利用者Snapshot snapshot, ISnapshotMetadata metadata, CancellationToken cancellationToken)
        {
            氏名 = snapshot.氏名;
            return Task.FromResult(0);
        }
    }

    [SnapshotVersion("利用者", 1)]
    public class 利用者Snapshot : ISnapshot
    {
        public 利用者のID 利用者のID { get; set; }
        public 氏名 氏名 { get; set; }
    }

    [EventVersion("利用者を登録した", 1)]
    public class 利用者を登録した : AggregateEvent<利用者, 利用者のID>
    {
        public 氏名 氏名 { get; set; }

        public 利用者を登録した() : base() { }

        public 利用者を登録した(氏名 _氏名) : this() => 氏名 = _氏名;
    }
}
