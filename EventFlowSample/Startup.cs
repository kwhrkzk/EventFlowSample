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
using EventFlow.EventStores.EventStore.Extensions;
using System.Net;
using EventStore.ClientAPI;
using EventFlow.PostgreSql.Extensions;
using EventFlow.PostgreSql.Connections;
using EventFlow.PostgreSql;
using EventFlow.PostgreSql.EventStores;
using EventFlow.Sql.Migrations;
using EventFlow.PostgreSql.SnapshotStores;

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
                    //.UseEventStoreEventStore(new Uri("tcp://admin:changeit@10.0.0.12:1113"), ConnectionSettings.Default)
                    //.UseFilesEventStore(FilesEventStoreConfiguration.Create(@".\store"))
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
                        typeof(利用者AllQueryHandler),
                        //typeof(本DTOQueryHandlerForPostgres),
                        //typeof(利用者AllQueryHandlerForPostgres),
                    })
                    .AddSnapshots(new[] {
                        typeof(利用者Snapshot),
                        typeof(本Snapshot),
                    })
                    .UseInMemoryReadStoreFor<利用者ReadModel>()
                    .UseInMemoryReadStoreFor<本ReadModel>()
                    .UseInMemorySnapshotStore()
                    //.ConfigurePostgreSql(PostgreSqlConfiguration.New.SetConnectionString("Server=10.0.0.12;Port=5432;User ID=postgres;Database=eventflow;password=post;Enlist=true"))
                    //.UsePostgreSqlReadModel<利用者ReadModelForPostgres>()
                    //.UsePostgreSqlReadModel<本ReadModelForPostgres>()
                    //.UsePostgreSqlSnapshotStore()
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

            //MigrateDatabase(container);

            var cb = container.Resolve<ICommandBus>();
            cb.PublishAsync(new 本を登録するCommand(本のID.New, 本のタイトル.Create("実践ドメイン駆動設計")), CancellationToken.None).ConfigureAwait(false);
            cb.PublishAsync(new 本を登録するCommand(本のID.New, 本のタイトル.Create(".NETのエンタープライズ アプリケーションアーキテクチャ")), CancellationToken.None).ConfigureAwait(false);
            cb.PublishAsync(new 利用者を登録するCommand(利用者のID.New, 氏名.Create("田中太郎")), CancellationToken.None);
            cb.PublishAsync(new 利用者を登録するCommand(利用者のID.New, 氏名.Create("鈴木次郎")), CancellationToken.None);

            return new AutofacServiceProvider(container);
        }

        private void MigrateDatabase(IContainer container)
        {
            var databaseMigrator = container.Resolve<IPostgreSqlDatabaseMigrator>();
            EventFlowSnapshotStoresPostgreSql.MigrateDatabase(databaseMigrator);
            //EventFlowEventStoresPostgreSql.MigrateDatabase(databaseMigrator);
            databaseMigrator.MigrateDatabaseUsingScripts(new[]
            {
                new SqlScript(
                    "本ReadModel",
@"CREATE TABLE IF NOT EXISTS ""ReadModel-本""(
    本のID Varchar(64),
    本のタイトル Varchar(64),
    利用者のID Varchar(64),
    貸出期間自 Timestamp WITH TIME ZONE,
    貸出期間至 Timestamp WITH TIME ZONE,

    -- -------------------------------------------------
    Id bigint GENERATED BY DEFAULT AS IDENTITY,
    AggregateId Varchar(64) NOT NULL,
    CreateTime Timestamp WITH TIME ZONE NOT NULL,
    UpdatedTime Timestamp WITH TIME ZONE NOT NULL,
    LastAggregateSequenceNumber int NOT NULL,
    CONSTRAINT ""PK_ReadModel-本"" PRIMARY KEY
    (
        Id
    )
);

CREATE INDEX  IF NOT EXISTS ""IX_ReadModel-本_AggregateId"" ON ""ReadModel-本""
(
    AggregateId
);
            "),
                new SqlScript(
                    "利用者ReadModel",
@"CREATE TABLE IF NOT EXISTS ""ReadModel-利用者""(
    利用者のID Varchar(64),
    氏名 Varchar(64),

    -- -------------------------------------------------
    Id bigint GENERATED BY DEFAULT AS IDENTITY,
    AggregateId Varchar(64) NOT NULL,
    CreateTime Timestamp WITH TIME ZONE NOT NULL,
    UpdatedTime Timestamp WITH TIME ZONE NOT NULL,
    LastAggregateSequenceNumber int NOT NULL,
    CONSTRAINT ""PK_ReadModel-利用者"" PRIMARY KEY
    (
        Id
    )
);

CREATE INDEX  IF NOT EXISTS ""IX_ReadModel-利用者_AggregateId"" ON ""ReadModel-利用者""
(
    AggregateId
);
            "),
            });
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
