using Autofac;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.AzureRepositories;
using MarginTrading.BrokerBase;
using MarginTrading.Core;
using MarginTrading.Core.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.OrderHistoryBroker
{
    public class Startup : BrokerStartupBase<Settings>
    {
        public Startup(IHostingEnvironment env) : base(env)
        {
        }

        protected override string ApplicationName => "MarginTradingOrderHistoryBroker";

        protected override void RegisterCustomServices(IServiceCollection services, ContainerBuilder builder, IReloadingManager<Settings> settingsRoot, ILog log, bool isLive)
        {
            MarginSettings SetLive(MarginSettings s)
            {
                s.IsLive = isLive;
                return s;
            }

            var settings = isLive
                ? settingsRoot.Nested(s => SetLive(s.MtBackend.MarginTradingLive))
                : settingsRoot.Nested(s => SetLive(s.MtBackend.MarginTradingDemo));

            builder.RegisterInstance(settings).SingleInstance();
            builder.RegisterInstance(settings.CurrentValue).SingleInstance();
            builder.RegisterType<Application>().As<IBrokerApplication>().SingleInstance();

            builder.Register<IMarginTradingOrdersHistoryRepository>(ctx =>
                AzureRepoFactories.MarginTrading.CreateOrdersHistoryRepository(settings.Nested(s => s.Db.HistoryConnString), log)
            ).SingleInstance();
        }
    }
}