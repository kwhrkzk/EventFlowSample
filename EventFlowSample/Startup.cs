using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Domain;
using EventFlow;
using EventFlow.AspNetCore.Extensions;
using EventFlow.AspNetCore.Middlewares;
using EventFlow.Autofac.Extensions;
using EventFlow.Configuration;
using EventFlow.EventStores.Files;
using EventFlow.Extensions;
using EventFlow.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using EventFlow.ValueObjects;
using EventFlow.ReadStores.InMemory;
using Domain.利用者;
using Domain.本;
using EventFlowSample.Pages.Book;
using System.Threading;
using EventFlow.Core;

namespace EventFlowSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(services);

            IContainer container = EventFlowOptions.New
                    .UseAutofacContainerBuilder(containerBuilder) // Must be the first line!
                    .RegisterServices(serviceRegistration => {
                        serviceRegistration.Register<IJsonSerializer, MyJsonSerializer>();
                    })
                    .AddAspNetCore((AspNetCoreEventFlowOptions op) =>
                        op.UseLogging()
                        .UseDefaults()
                        //.UseMvcJsonOptions()
                        //.AddDefaultMetadataProviders()
                        //.AddRequestHeadersMetadata()
                        //.AddUriMetadata()
                        //.AddUserHostAddressMetadata()
                        //.RunBootstrapperOnHostStartup()
                    )
                    .AddEvents(new[] {
                        typeof(利用者を登録した),
                        typeof(本を登録した),
                        typeof(本を借りた),
                        typeof(本を返した),
                        typeof(本を破棄した),
                    })
                    .AddCommands(new[] {
                        typeof(利用者を登録するCommand),
                        typeof(本を登録するCommand),
                        typeof(本を借りるCommand),
                        typeof(本を返すCommand),
                        typeof(本を破棄するCommand),
                    })
                    .AddCommandHandlers(new[] {
                        typeof(利用者を登録するCommandHandler),
                        typeof(本を登録するCommandHandler),
                        typeof(本を借りるCommandHandler),
                        typeof(本を返すCommandHandler),
                        typeof(本を破棄するCommandHandler),
                    })
                    .AddQueryHandlers(new[] {
                        typeof(本DTOQueryHandler),
                    })
                    .UseInMemoryReadStoreFor<利用者ReadModel>()
                    .UseInMemoryReadStoreFor<本ReadModel>()
                    .UseFilesEventStore(FilesEventStoreConfiguration.Create(@".\store"))
                    .Configure((EventFlowConfiguration c) =>
                    {
                        c.IsAsynchronousSubscribersEnabled = true;
                    })
                    .ConfigureJson(json => json
                        .AddSingleValueObjects()
                        //.AddConverter<SingleValueObjectConverter>()
                        //.Configure((JsonSerializerSettings s) => { })
                    )
                    .CreateContainer();

            var cb = container.Resolve<ICommandBus>();
            cb.PublishAsync(new 本を登録するCommand(本のID.New, 本のタイトル.Create("実践ドメイン駆動設計")), CancellationToken.None).ConfigureAwait(false);
            cb.PublishAsync(new 本を登録するCommand(本のID.New, 本のタイトル.Create(".NETのエンタープライズ アプリケーションアーキテクチャ")), CancellationToken.None).ConfigureAwait(false);
            cb.PublishAsync(new 利用者を登録するCommand(利用者のID.New, 氏名.Create("田中太郎")), CancellationToken.None);
            cb.PublishAsync(new 利用者を登録するCommand(利用者のID.New, 氏名.Create("鈴木次郎")), CancellationToken.None);

            return new AutofacServiceProvider(container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMiddleware<CommandPublishMiddleware>();
            app.UseMvc(routes => {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
    public class MyJsonSerializer : IJsonSerializer
    {
        public object Deserialize(string json, Type type)
        {
            try
            {
                return Utf8Json.JsonSerializer.NonGeneric.Deserialize(type, json);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public T Deserialize<T>(string json) => Utf8Json.JsonSerializer.Deserialize<T>(json);
        public string Serialize(object obj, bool indented = false) => Utf8Json.JsonSerializer.ToJsonString(obj);
    }
}
