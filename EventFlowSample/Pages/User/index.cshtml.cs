using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Domain;
using Domain.利用者;
using EventFlow;
using EventFlow.Configuration;
using EventFlow.Queries;
using EventFlow.ReadStores.InMemory.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventFlowSample.Pages.User
{
    public class indexModel : PageModel
    {
        private IQueryProcessor QueryProcessor { get; }
        public indexModel(IQueryProcessor _queryProcessor) => QueryProcessor = _queryProcessor;

        [BindProperty]
        public IReadOnlyCollection<利用者ReadModel> 利用者一覧 { get; set; }

        public async void OnGet() => 利用者一覧 = await QueryProcessor.ProcessAsync(new InMemoryQuery<利用者ReadModel>(_ => true), CancellationToken.None);
    }
}