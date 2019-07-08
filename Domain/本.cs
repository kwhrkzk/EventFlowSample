using Domain.利用者;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Core;
using EventFlow.EventStores;
using EventFlow.ReadStores;
using EventFlow.Snapshots;
using EventFlow.Snapshots.Strategies;
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
        public static 貸出期間 Create(貸出期間自 _貸出期間自, 貸出期間至 _貸出期間至)
            => ((_貸出期間自 == null) || (_貸出期間至 == null)) ? null : new 貸出期間(_貸出期間自, _貸出期間至);
        public static 貸出期間 Create(DateTime? _貸出期間自, DateTime? _貸出期間至)
            => (_貸出期間自.HasValue && _貸出期間至.HasValue) ? new 貸出期間(new 貸出期間自(_貸出期間自.Value), new 貸出期間至(_貸出期間至.Value)) : null;

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

    public class 本 : SnapshotAggregateRoot<本, 本のID, 本Snapshot>
    {
        private 本を登録した状態 本を登録した状態 { get; } = new 本を登録した状態();
        private 本を借りた状態 本を借りた状態 { get; } = new 本を借りた状態();
        private 本を返した状態 本を返した状態 { get; } = new 本を返した状態();
        private 本を破棄した状態 本を破棄した状態 { get; } = new 本を破棄した状態();

        public 本のタイトル 本のタイトル { get; set; }
        public 貸出期間 貸出期間 { get; set; }
        public 利用者のID 利用者のID { get; set; }
        public 本(本のID id) : base(id, new MySnapshotStrategy())
        {
            Register(本を登録した状態);
            Register(本を借りた状態);
            Register(本を返した状態);
            Register(本を破棄した状態);
        }

        public void 本を登録する(本のタイトル _本のタイトル)
        {
            本のタイトル = _本のタイトル;
            Emit(new 本を登録した(_本のタイトル));
        }

        public void 本を借りる(貸出期間 _貸出期間, 利用者のID _利用者のID)
        {
            貸出期間 = _貸出期間;
            利用者のID = _利用者のID;
            Emit(new 本を借りた(_貸出期間, _利用者のID));
        }

        public void 本を返す()
        {
            貸出期間 = null;
            利用者のID = null;
            Emit(new 本を返した());
        }

        public void 本を破棄する()
        {
            本のタイトル = null;
            貸出期間 = null;
            利用者のID = null;
            Emit(new 本を破棄した());
        }

        protected override Task<本Snapshot> CreateSnapshotAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new 本Snapshot() { 本のID = Id, 本のタイトル = 本のタイトル, 利用者のID = 利用者のID, 貸出期間 = 貸出期間 });
        }

        protected override Task LoadSnapshotAsync(本Snapshot snapshot, ISnapshotMetadata metadata, CancellationToken cancellationToken)
        {
            本のタイトル = snapshot.本のタイトル;
            利用者のID = snapshot.利用者のID;
            貸出期間 = snapshot.貸出期間;
            return Task.FromResult(0);
        }
    }

    public class MySnapshotStrategy : ISnapshotStrategy
    {
        public Task<bool> ShouldCreateSnapshotAsync(ISnapshotAggregateRoot snapshotAggregateRoot, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }

    [SnapshotVersion("本", 1)]
    public class 本Snapshot : ISnapshot
    {
        public 本のID 本のID { get; set; }
        public 本のタイトル 本のタイトル { get; set; }
        public 貸出期間 貸出期間 { get; set; }
        public 利用者のID 利用者のID { get; set; }
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

    [EventVersion("本を破棄した", 1)]
    public class 本を破棄した : AggregateEvent<本, 本のID> { }

    public class 本を破棄した状態 : AggregateState<本, 本のID, 本を破棄した状態>, IApply<本を破棄した>
    {
        public 本を破棄した状態() : base() { }

        public void Apply(本を破棄した aggregateEvent)
        {
        }
    }
}
