using Autofac;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.AzureRepositories;
using MarginTrading.AzureRepositories.Reports;
using MarginTrading.Core;
using MarginTrading.Core.Clients;
using MarginTrading.Core.Monitoring;
using MarginTrading.Core.Settings;
using MarginTrading.Services;

namespace MarginTrading.Backend.Modules
{
	public class BackendRepositoriesModule : Module
	{
		private readonly IReloadingManager<MarginSettings> _settings;
		private readonly ILog _log;

		public BackendRepositoriesModule(IReloadingManager<MarginSettings> settings, ILog log)
		{
			_settings = settings;
			_log = log;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterInstance(_log)
				.As<ILog>()
				.SingleInstance();

		    builder.Register<IMarginTradingOperationsLogRepository>(ctx =>
		        new MarginTradingOperationsLogRepository(AzureTableStorage<OperationLogEntity>.Create(_settings.Nested(s => s.Db.LogsConnString),
		                "MarginTradingBackendOperationsLog", _log))
		    ).SingleInstance();

			builder.Register<IClientSettingsRepository>(ctx =>
				AzureRepoFactories.Clients.CreateTraderSettingsRepository(_settings.Nested(s => s.Db.ClientPersonalInfoConnString), _log)
			).SingleInstance();

			builder.Register<IClientAccountsRepository>(ctx =>
				AzureRepoFactories.Clients.CreateClientsRepository(_settings.Nested(s => s.Db.ClientPersonalInfoConnString), _log)
			).SingleInstance();

			builder.Register<IMarginTradingAccountsRepository>(ctx =>
				AzureRepoFactories.MarginTrading.CreateAccountsRepository(_settings.Nested(s => s.Db.MarginTradingConnString), _log)
			).SingleInstance();

			builder.Register<IMarginTradingOrdersHistoryRepository>(ctx =>
				AzureRepoFactories.MarginTrading.CreateOrdersHistoryRepository(_settings.Nested(s => s.Db.HistoryConnString), _log)
			).SingleInstance();

			builder.Register<IMarginTradingAccountHistoryRepository>(ctx =>
				AzureRepoFactories.MarginTrading.CreateAccountHistoryRepository(_settings.Nested(s => s.Db.HistoryConnString), _log)
			).SingleInstance();

			builder.Register<IMatchingEngineRoutesRepository>(ctx =>
				AzureRepoFactories.MarginTrading.CreateMatchingEngineRoutesRepository(_settings.Nested(s => s.Db.MarginTradingConnString), _log)
			).SingleInstance();

			builder.Register<IMarginTradingConditionRepository>(ctx =>
				AzureRepoFactories.MarginTrading.CreateTradingConditionsRepository(_settings.Nested(s => s.Db.MarginTradingConnString), _log)
			).SingleInstance();

			builder.Register<IMarginTradingAccountGroupRepository>(ctx =>
				AzureRepoFactories.MarginTrading.CreateAccountGroupRepository(_settings.Nested(s => s.Db.MarginTradingConnString), _log)
			).SingleInstance();

			builder.Register<IMarginTradingAccountAssetRepository>(ctx =>
				AzureRepoFactories.MarginTrading.CreateAccountAssetsRepository(_settings.Nested(s => s.Db.MarginTradingConnString), _log)
			).SingleInstance();

			builder.Register<IMarginTradingAssetsRepository>(ctx =>
				AzureRepoFactories.MarginTrading.CreateAssetsRepository(_settings.Nested(s => s.Db.DictsConnString), _log)
			).SingleInstance();

			builder.Register<IMarginTradingBlobRepository>(ctx =>
				AzureRepoFactories.MarginTrading.CreateBlobRepository(_settings.Nested(s => s.Db.StateConnString))
			).SingleInstance();

			builder.Register<IServiceMonitoringRepository>(ctx =>
				AzureRepoFactories.Monitoring.CreateServiceMonitoringRepository(_settings.Nested(s => s.Db.SharedStorageConnString), _log)
			).SingleInstance();

			builder.Register<IAppGlobalSettingsRepositry>(ctx =>
				AzureRepoFactories.Settings.CreateAppGlobalSettingsRepository(_settings.Nested(s => s.Db.ClientPersonalInfoConnString), _log)
			).SingleInstance();

			builder.Register<IAccountsStatsReportsRepository>(ctx =>
				AzureRepoFactories.MarginTrading.CreateAccountsStatsReportsRepository(_settings.Nested(s => s.Db.ReportsConnString), _log)
			).SingleInstance();

			builder.Register<IAccountsReportsRepository>(ctx =>
				AzureRepoFactories.MarginTrading.CreateAccountsReportsRepository(_settings.Nested(s => s.Db.ReportsConnString), _log)
			).SingleInstance();

			builder.RegisterType<MatchingEngineInMemoryRepository>()
				.As<IMatchingEngineRepository>()
				.SingleInstance();
		}
	}
}
