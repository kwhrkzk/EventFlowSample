using Domain.利用者;
using Domain.本;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Commands;
using EventFlow.Core;
using EventFlow.PostgreSql.Connections;
using EventFlow.PostgreSql.ReadStores;
using EventFlow.PostgreSql.ReadStores.Attributes;
using EventFlow.Queries;
using EventFlow.ReadStores;
using EventFlow.ReadStores.InMemory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventFlowSample
{
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

    public class 利用者ReadModel : IReadModel, IAmReadModelFor<利用者, 利用者のID, 利用者を登録した>
    {
        public 利用者のID 利用者のID { get; private set; }
        public 氏名 氏名 { get; private set; }

        public void Apply(IReadModelContext context, IDomainEvent<利用者, 利用者のID, 利用者を登録した> domainEvent)
        {
            利用者のID = domainEvent.AggregateIdentity;
            氏名 = domainEvent.AggregateEvent.氏名;
        }

        public 利用者 To利用者() => new 利用者(利用者のID) { 氏名 = 氏名 };
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
        public 貸出期間自 貸出期間自 { get; set; }
        public 貸出期間至 貸出期間至 { get; set; }

        public void Apply(IReadModelContext context, IDomainEvent<本, 本のID, 本を登録した> domainEvent)
        {
            本のID = domainEvent.AggregateIdentity;
            本のタイトル = domainEvent.AggregateEvent.本のタイトル;
        }

        public void Apply(IReadModelContext context, IDomainEvent<本, 本のID, 本を借りた> domainEvent)
        {
            本のID = domainEvent.AggregateIdentity;
            利用者のID = domainEvent.AggregateEvent.利用者のID;
            貸出期間自 = domainEvent.AggregateEvent.貸出期間.貸出期間自;
            貸出期間至 = domainEvent.AggregateEvent.貸出期間.貸出期間至;
        }

        public void Apply(IReadModelContext context, IDomainEvent<本, 本のID, 本を返した> domainEvent)
        {
            本のID = domainEvent.AggregateIdentity;
            利用者のID = null;
            貸出期間自 = null;
            貸出期間至 = null;
        }

        public void Apply(IReadModelContext context, IDomainEvent<本, 本のID, 本を破棄した> domainEvent)
        {
            context.MarkForDeletion();
        }
    }

    [Table("ReadModel-本")]
    public class 本ReadModelForPostgres : PostgreSqlReadModel
        , IAmReadModelFor<本, 本のID, 本を登録した>
        , IAmReadModelFor<本, 本のID, 本を借りた>
        , IAmReadModelFor<本, 本のID, 本を返した>
        , IAmReadModelFor<本, 本のID, 本を破棄した>
    {
        public string 本のID { get; set; }
        public string 本のタイトル { get; set; }

        public string 利用者のID { get; set; }
        public DateTime? 貸出期間自 { get; set; }
        public DateTime? 貸出期間至 { get; set; }

        public 本ReadModelForPostgres() : base() { }

        public void Apply(IReadModelContext context, IDomainEvent<本, 本のID, 本を登録した> domainEvent)
        {
            本のID = domainEvent.AggregateIdentity.Value;
            本のタイトル = domainEvent.AggregateEvent.本のタイトル.Value;
        }

        public void Apply(IReadModelContext context, IDomainEvent<本, 本のID, 本を借りた> domainEvent)
        {
            本のID = domainEvent.AggregateIdentity.Value;
            利用者のID = domainEvent.AggregateEvent.利用者のID.Value;
            貸出期間自 = domainEvent.AggregateEvent.貸出期間.貸出期間自.Value;
            貸出期間至 = domainEvent.AggregateEvent.貸出期間.貸出期間至.Value;
        }

        public void Apply(IReadModelContext context, IDomainEvent<本, 本のID, 本を返した> domainEvent)
        {
            本のID = domainEvent.AggregateIdentity.Value;
            利用者のID = null;
            貸出期間自 = null;
            貸出期間至 = null;
        }

        public void Apply(IReadModelContext context, IDomainEvent<本, 本のID, 本を破棄した> domainEvent)
        {
            context.MarkForDeletion();
        }

        public 本 To本()
            => new 本(Domain.本.本のID.With(本のID)) {
                本のタイトル = Domain.本.本のタイトル.Create(本のタイトル),
                利用者のID = (利用者のID == null) ? null : Domain.利用者.利用者のID.With(利用者のID),
                貸出期間 = ((貸出期間自 == null) || (貸出期間至 == null)) ? null : 貸出期間.Create(貸出期間自, 貸出期間至),
            };
    }

    [Table("ReadModel-利用者")]
    public class 利用者ReadModelForPostgres : PostgreSqlReadModel, IAmReadModelFor<利用者, 利用者のID, 利用者を登録した>
    {
        public string 利用者のID { get; set; }
        public string 氏名 { get; set; }

        public 利用者ReadModelForPostgres() :base() { }

        public void Apply(IReadModelContext context, IDomainEvent<利用者, 利用者のID, 利用者を登録した> domainEvent)
        {
            利用者のID = domainEvent.AggregateIdentity.Value;
            氏名 = domainEvent.AggregateEvent.氏名.Value;
        }

        public 利用者 To利用者()
            => new 利用者(Domain.利用者.利用者のID.With(利用者のID))
            {
                氏名 = Domain.利用者.氏名.Create(氏名),
            };
    }

    public class 利用者AllQuery : IQuery<IReadOnlyCollection<利用者>>
    {
    }

    public class 利用者AllQueryHandler : IQueryHandler<利用者AllQuery, IReadOnlyCollection<利用者>>
    {
        private IInMemoryReadStore<利用者ReadModel> InMemoryReadStore { get; }

        public 利用者AllQueryHandler(IInMemoryReadStore<利用者ReadModel> _inMemoryReadStore) => InMemoryReadStore = _inMemoryReadStore;

        public async Task<IReadOnlyCollection<利用者>> ExecuteQueryAsync(利用者AllQuery query, CancellationToken cancellationToken)
        {
            var list = (await InMemoryReadStore.FindAsync(_ => true, cancellationToken)).Select(m => m.To利用者());

            return list.ToList().AsReadOnly();
        }
    }

    public class 利用者AllQueryHandlerForPostgres : IQueryHandler<利用者AllQuery, IReadOnlyCollection<利用者>>
    {
        private IPostgreSqlConnection PostgreSqlConnection { get; }

        public 利用者AllQueryHandlerForPostgres(IPostgreSqlConnection _postgreSqlConnection) => PostgreSqlConnection = _postgreSqlConnection;

        public async Task<IReadOnlyCollection<利用者>> ExecuteQueryAsync(利用者AllQuery query, CancellationToken cancellationToken)
        {
            var list = (await PostgreSqlConnection.QueryAsync<利用者ReadModelForPostgres>(Label.Named("postgresql-利用者ReadModel"), cancellationToken, "SELECT * FROM \"ReadModel-利用者\"")).Select(m => m.To利用者());

            return list.ToList().AsReadOnly();
        }
    }

    public class 本DTO : IReadModel
    {
        [PostgreSqlReadModelIdentityColumn]
        public 本のID 本のID { get; set; }
        public 本のタイトル 本のタイトル { get; set; }
        public 貸出期間 貸出期間 { get; set; }
        public 利用者のID 利用者のID { get; set; }
        public 氏名 氏名 { get; set; }
    }

    public class 本DTOQuery : IQuery<IReadOnlyCollection<本DTO>>
    {
    }

    public class 本DTOQueryHandler : IQueryHandler<本DTOQuery, IReadOnlyCollection<本DTO>>
    {
        private IInMemoryReadStore<本ReadModel> 本Store { get; }
        private IInMemoryReadStore<利用者ReadModel> 利用者Store { get; }
        public 本DTOQueryHandler(IInMemoryReadStore<本ReadModel> _本Store, IInMemoryReadStore<利用者ReadModel> _利用者Store)
        {
            本Store = _本Store;
            利用者Store = _利用者Store;
        }

        public async Task<IReadOnlyCollection<本DTO>> ExecuteQueryAsync(本DTOQuery query, CancellationToken cancellationToken)
        {
            var 本Model = await 本Store.FindAsync(model => true, cancellationToken);
            var 利用者Model = await 利用者Store.FindAsync(model => true, cancellationToken);

            var list = from 本 in 本Model
                       join _利用者 in 利用者Model on 本.利用者のID equals _利用者.利用者のID into 利用者Join
                       from 利用者 in 利用者Join.DefaultIfEmpty()
                       select new 本DTO { 本のID = 本.本のID, 本のタイトル = 本.本のタイトル, 利用者のID = 本?.利用者のID, 貸出期間 = 貸出期間.Create(本?.貸出期間自, 本?.貸出期間至), 氏名 = 利用者?.氏名 };

            return list.ToList().AsReadOnly();
        }
    }

    public class 本DTOQueryHandlerForPostgres : IQueryHandler<本DTOQuery, IReadOnlyCollection<本DTO>>
    {
        private IPostgreSqlConnection PostgreSqlConnection { get; }

        public 本DTOQueryHandlerForPostgres(IPostgreSqlConnection _postgreSqlConnection) => PostgreSqlConnection = _postgreSqlConnection;

        public async Task<IReadOnlyCollection<本DTO>> ExecuteQueryAsync(本DTOQuery query, CancellationToken cancellationToken)
        {
            var 本Model = (await PostgreSqlConnection.QueryAsync<本ReadModelForPostgres>(Label.Named("postgresql-本ReadModel"), cancellationToken, "SELECT * FROM \"ReadModel-本\"")).Select(m => m.To本());
            var 利用者Model = (await PostgreSqlConnection.QueryAsync<利用者ReadModelForPostgres>(Label.Named("postgresql-利用者ReadModel"), cancellationToken, "SELECT * FROM \"ReadModel-利用者\"")).Select(m => m.To利用者());

            var list = from 本 in 本Model
                       join _利用者 in 利用者Model on 本.利用者のID equals _利用者.Id into 利用者Join
                       from 利用者 in 利用者Join.DefaultIfEmpty()
                       select new 本DTO { 本のID = 本.Id, 本のタイトル = 本.本のタイトル, 利用者のID = 本?.利用者のID, 貸出期間 = 本.貸出期間, 氏名 = 利用者?.氏名 };

            return list.ToList().AsReadOnly();
        }
    }
}
