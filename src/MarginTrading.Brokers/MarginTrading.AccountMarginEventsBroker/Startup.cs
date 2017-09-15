using Autofac;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.AccountMarginEventsBroker.AzureRepositories;
using MarginTrading.BrokerBase;
using MarginTrading.Core.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MarginTrading.AccountMarginEventsBroker
{
    public class Startup : BrokerStartupBase<Settings>
    {
        protected override string ApplicationName => "MarginTradingAccountMarginEventsBroker";

        public Startup(IHostingEnvironment env) : base(env)
        {
        }


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
            builder.RegisterInstance(settings.CurrentValue).SingleInstance();
            builder.RegisterInstance(settings).SingleInstance();
            builder.RegisterType<Application>().As<IBrokerApplication>().SingleInstance();

            builder.RegisterType<AccountMarginEventsReportsRepository>().As<IAccountMarginEventsReportsRepository>()
                .SingleInstance();
        }
    }
}