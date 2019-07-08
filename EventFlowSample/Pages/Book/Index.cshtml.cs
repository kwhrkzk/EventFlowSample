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
using EventFlow.Core;
using EventFlow.PostgreSql.Connections;
using EventFlow.PostgreSql.ReadModels;
using EventFlow.PostgreSql.ReadStores;
using EventFlow.PostgreSql.ReadStores.Attributes;
using EventFlow.Queries;
using EventFlow.ReadStores;
using EventFlow.ReadStores.InMemory;
using EventFlow.ReadStores.InMemory.Queries;
using EventFlow.Sql.ReadModels;
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
            var 利用者 = (await QueryProcessor.ProcessAsync(new 利用者AllQuery(), CancellationToken.None)).First();

            await CommandBus
                    .PublishAsync(
                        new 本を借りるCommand(本のID.With(id), 貸出期間.今日から２週間, 利用者.Id),
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
}