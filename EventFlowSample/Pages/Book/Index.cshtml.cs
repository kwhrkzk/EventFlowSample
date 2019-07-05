using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Domain.利用者;
using Domain.本;
using EventFlow;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Queries;
using EventFlow.ReadStores;
using EventFlow.ReadStores.InMemory;
using EventFlow.ReadStores.InMemory.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventFlowSample.Pages.Book
{
    public class IndexModel : PageModel
    {
        private ICommandBus CommandBus { get; }
        private IQueryProcessor QueryProcessor { get; }
        public IndexModel(ICommandBus _commandBus, IQueryProcessor _queryProcessor)
        {
            CommandBus = _commandBus;
            QueryProcessor = _queryProcessor;
        }

        [BindProperty]
        public IReadOnlyCollection<本DTO> 本一覧 { get; set; }

        public async Task OnGetAsync()
        {
            本一覧 = await QueryProcessor.ProcessAsync(new 本DTOQuery(), CancellationToken.None);
        }

        public async Task OnPostBorrowAsync(string id)
        {
            var 利用者 = (await QueryProcessor.ProcessAsync(new InMemoryQuery<利用者ReadModel>(m => true), CancellationToken.None)).First();

            await CommandBus
                    .PublishAsync(
                        new 本を借りるCommand(本のID.With(id), 貸出期間.今日から２週間, 利用者.利用者のID),
                        CancellationToken.None).ConfigureAwait(false);

            本一覧 = await QueryProcessor.ProcessAsync(new 本DTOQuery(), CancellationToken.None);
        }

        public async Task OnPostReturnAsync(string id)
        {
            await CommandBus
                    .PublishAsync(
                        new 本を返すCommand(本のID.With(id)),
                        CancellationToken.None).ConfigureAwait(false);

            本一覧 = await QueryProcessor.ProcessAsync(new 本DTOQuery(), CancellationToken.None);
        }

        public async Task OnPostDiscardAsync(string id)
        {
            await CommandBus
                    .PublishAsync(
                        new 本を破棄するCommand(本のID.With(id)),
                        CancellationToken.None).ConfigureAwait(false);

            本一覧 = await QueryProcessor.ProcessAsync(new 本DTOQuery(), CancellationToken.None);
        }
    }

    public class 本DTO : IReadModel
    {
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
                       join _利用者 in 利用者Model on 本.利用者のID equals _利用者.利用者のID into 利用者Join from 利用者 in 利用者Join.DefaultIfEmpty()
                       select new 本DTO { 本のID = 本.本のID, 本のタイトル = 本.本のタイトル, 利用者のID = 本?.利用者のID, 貸出期間 = 本?.貸出期間, 氏名 = 利用者?.氏名 };

            return list.ToList().AsReadOnly();
        }
    }
}