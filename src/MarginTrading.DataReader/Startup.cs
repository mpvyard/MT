using System;
using System.IO;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Flurl.Http;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using MarginTrading.Common.Extensions;
using MarginTrading.DataReader.Filters;
using MarginTrading.DataReader.Infrastructure;
using MarginTrading.DataReader.Middleware;
using MarginTrading.DataReader.Modules;
using MarginTrading.DataReader.Settings;
using MarginTrading.Services.Infrastructure;
using MarginTrading.Services.Notifications;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Annotations;

#pragma warning disable 1591

namespace MarginTrading.DataReader
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; set; }

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.dev.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            Environment = env;
        }

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = new LoggerFactory()
                .AddConsole(LogLevel.Error)
                .AddDebug(LogLevel.Error);

            services.AddSingleton(loggerFactory);
            services.AddLogging();
            services.AddSingleton(Configuration);
            services.AddMvc(options => options.Filters.Add(typeof(MarginTradingEnabledFilter)));
            services.AddAuthentication(KeyAuthOptions.AuthenticationScheme)
                .AddScheme<KeyAuthOptions, KeyAuthHandler>(KeyAuthOptions.AuthenticationScheme, "", options => { });

            var isLive = Configuration.IsLive();

            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration("v1", $"MarginTrading_DataReader_Api_{(isLive ? "Live" : "Demo")}");
                options.OperationFilter<ApiKeyHeaderOperationFilter>();
                options.OperationFilter<CustomOperationIdOperationFilter>();
                options.SchemaFilter<FixResponseValueTypesNullabilitySchemaFilter>();
            });

            var builder = new ContainerBuilder();

            var readerSettings = Environment.IsDevelopment()
                ? Configuration.Get<AppSettings>()
                : SettingsProcessor.Process<AppSettings>(Configuration["SettingsUrl"].GetStringAsync().Result);

            var settings = isLive
                ? readerSettings.MtDataReader.Live
                : readerSettings.MtDataReader.Demo;
            settings.IsLive = isLive;
            settings.Env = isLive ? "Live" : "Demo";

            Console.WriteLine($"IsLive: {settings.IsLive}");

            SetupLoggers(services, readerSettings, settings);

            RegisterModules(builder, settings);

            builder.Populate(services);
            ApplicationContainer = builder.Build();
            return new AutofacServiceProvider(ApplicationContainer);
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            app.UseLykkeMiddleware("MarginTradingDataReader",
#if DEBUG
                ex => ex.ToString());
#else
                ex => new { ErrorMessage = "Technical problem" });
#endif
            app.UseAuthentication();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwagger(DocumentFilter, "swagger/{apiVersion}/swagger-no-api-key.json");
            app.UseSwaggerUi();

            appLifetime.ApplicationStopped.Register(() => ApplicationContainer.Dispose());

            var settings = app.ApplicationServices.GetService<DataReaderSettings>();

            appLifetime.ApplicationStarted.Register(() =>
            {
                if (!string.IsNullOrEmpty(settings.ApplicationInsightsKey))
                {
                    TelemetryConfiguration.Active.InstrumentationKey =
                        settings.ApplicationInsightsKey;
                }
            });

            appLifetime.ApplicationStopping.Register(() => { });
        }

        /// <summary>
        /// If generating swagger without api-key - strip it.
        /// </summary>
        /// <remarks>
        /// This is a nasty workaround for autorest generator not to create apiKey parameters for every method.
        /// </remarks>
        private void DocumentFilter(HttpRequest httpRequest, SwaggerDocument swaggerDocument)
        {
            foreach (var path in swaggerDocument.Paths.Values)
            {
                path.Get.Parameters?.Remove(path.Get.Parameters.First(p => p.Name == KeyAuthOptions.DefaultHeaderName));
            }
        }

        private void RegisterModules(ContainerBuilder builder, DataReaderSettings settings)
        {
            builder.RegisterModule(new DataReaderSettingsModule(settings));
            builder.RegisterModule(new DataReaderRepositoriesModule(settings, LogLocator.CommonLog));
            builder.RegisterModule(new DataReaderServicesModule());
        }

        private static void SetupLoggers(IServiceCollection services, AppSettings mtSettings,
            DataReaderSettings settings)
        {
            var consoleLogger = new LogToConsole();

            var commonSlackService =
                services.UseSlackNotificationsSenderViaAzureQueue(mtSettings.SlackNotifications.AzureQueue,
                    new LogToConsole());

            var slackService =
                new MtSlackNotificationsSender(commonSlackService, "MT DataReader", settings.Env);

            var log = services.UseLogToAzureStorage(settings.Db.LogsConnString, slackService,
                "MarginTradingDataReaderLog", consoleLogger);

            LogLocator.CommonLog = log;
        }
    }
}