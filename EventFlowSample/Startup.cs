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
using Domain.���p��;
using Domain.�{;
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
                        typeof(���p�҂�o�^����),
                        typeof(�{��o�^����),
                        typeof(�{���؂肽),
                        typeof(�{��Ԃ���),
                        typeof(�{��j������),
                    })
                    .AddCommands(new[] {
                        typeof(���p�҂�o�^����Command),
                        typeof(�{��o�^����Command),
                        typeof(�{���؂��Command),
                        typeof(�{��Ԃ�Command),
                        typeof(�{��j������Command),
                    })
                    .AddCommandHandlers(new[] {
                        typeof(���p�҂�o�^����CommandHandler),
                        typeof(�{��o�^����CommandHandler),
                        typeof(�{���؂��CommandHandler),
                        typeof(�{��Ԃ�CommandHandler),
                        typeof(�{��j������CommandHandler),
                    })
                    .AddQueryHandlers(new[] {
                        typeof(�{DTOQueryHandler),
                        typeof(���p��AllQueryHandler),
                        //typeof(�{DTOQueryHandlerForPostgres),
                        //typeof(���p��AllQueryHandlerForPostgres),
                    })
                    .AddSnapshots(new[] {
                        typeof(���p��Snapshot),
                        typeof(�{Snapshot),
                    })
                    .UseInMemoryReadStoreFor<���p��ReadModel>()
                    .UseInMemoryReadStoreFor<�{ReadModel>()
                    .UseInMemorySnapshotStore()
                    //.ConfigurePostgreSql(PostgreSqlConfiguration.New.SetConnectionString("Server=10.0.0.12;Port=5432;User ID=postgres;Database=eventflow;password=post;Enlist=true"))
                    //.UsePostgreSqlReadModel<���p��ReadModelForPostgres>()
                    //.UsePostgreSqlReadModel<�{ReadModelForPostgres>()
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
            cb.PublishAsync(new �{��o�^����Command(�{��ID.New, �{�̃^�C�g��.Create("���H�h���C���쓮�݌v")), CancellationToken.None).ConfigureAwait(false);
            cb.PublishAsync(new �{��o�^����Command(�{��ID.New, �{�̃^�C�g��.Create(".NET�̃G���^�[�v���C�Y �A�v���P�[�V�����A�[�L�e�N�`��")), CancellationToken.None).ConfigureAwait(false);
            cb.PublishAsync(new ���p�҂�o�^����Command(���p�҂�ID.New, ����.Create("�c�����Y")), CancellationToken.None);
            cb.PublishAsync(new ���p�҂�o�^����Command(���p�҂�ID.New, ����.Create("��؎��Y")), CancellationToken.None);

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
                    "�{ReadModel",
@"CREATE TABLE IF NOT EXISTS ""ReadModel-�{""(
    �{��ID Varchar(64),
    �{�̃^�C�g�� Varchar(64),
    ���p�҂�ID Varchar(64),
    �ݏo���Ԏ� Timestamp WITH TIME ZONE,
    �ݏo���Ԏ� Timestamp WITH TIME ZONE,

    -- -------------------------------------------------
    Id bigint GENERATED BY DEFAULT AS IDENTITY,
    AggregateId Varchar(64) NOT NULL,
    CreateTime Timestamp WITH TIME ZONE NOT NULL,
    UpdatedTime Timestamp WITH TIME ZONE NOT NULL,
    LastAggregateSequenceNumber int NOT NULL,
    CONSTRAINT ""PK_ReadModel-�{"" PRIMARY KEY
    (
        Id
    )
);

CREATE INDEX  IF NOT EXISTS ""IX_ReadModel-�{_AggregateId"" ON ""ReadModel-�{""
(
    AggregateId
);
            "),
                new SqlScript(
                    "���p��ReadModel",
@"CREATE TABLE IF NOT EXISTS ""ReadModel-���p��""(
    ���p�҂�ID Varchar(64),
    ���� Varchar(64),

    -- -------------------------------------------------
    Id bigint GENERATED BY DEFAULT AS IDENTITY,
    AggregateId Varchar(64) NOT NULL,
    CreateTime Timestamp WITH TIME ZONE NOT NULL,
    UpdatedTime Timestamp WITH TIME ZONE NOT NULL,
    LastAggregateSequenceNumber int NOT NULL,
    CONSTRAINT ""PK_ReadModel-���p��"" PRIMARY KEY
    (
        Id
    )
);

CREATE INDEX  IF NOT EXISTS ""IX_ReadModel-���p��_AggregateId"" ON ""ReadModel-���p��""
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
