using AzureStorage.Queue;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.AzureRepositories.Clients;
using MarginTrading.AzureRepositories.Monitoring;
using MarginTrading.AzureRepositories.Reports;
using MarginTrading.AzureRepositories.Settings;

namespace MarginTrading.AzureRepositories
{
    public class AzureRepoFactories
    {
        public static class Clients
        {
            public static ClientSettingsRepository CreateTraderSettingsRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new ClientSettingsRepository(
                    AzureTableStorage<ClientSettingsEntity>.Create(connStringReloadingManager, "TraderSettings", log));
            }

            public static ClientsRepository CreateClientsRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                const string tableName = "Traders";
                return new ClientsRepository(
                    AzureTableStorage<ClientAccountEntity>.Create(connStringReloadingManager, tableName, log),
                    AzureTableStorage<AzureIndex>.Create(connStringReloadingManager, tableName, log));
            }
        }

        public static class Monitoring
        {
            public static ServiceMonitoringRepository CreateServiceMonitoringRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new ServiceMonitoringRepository(AzureTableStorage<MonitoringRecordEntity>.Create(connStringReloadingManager, "Monitoring", log));
            }
        }

        public static class Settings
        {
            public static AppGlobalSettingsRepository CreateAppGlobalSettingsRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new AppGlobalSettingsRepository(AzureTableStorage<AppGlobalSettingsEntity>.Create(connStringReloadingManager, "Setup", log));
            }
        }

        public static class MarginTrading
        {
            public static MarginTradingConditionsRepository CreateTradingConditionsRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new MarginTradingConditionsRepository(AzureTableStorage<MarginTradingConditionEntity>.Create(connStringReloadingManager,
                    "MarginTradingConditions", log));
            }

            public static MarginTradingAccountGroupRepository CreateAccountGroupRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new MarginTradingAccountGroupRepository(AzureTableStorage<MarginTradingAccountGroupEntity>.Create(connStringReloadingManager,
                    "MarginTradingAccountGroups", log));
            }

            public static MarginTradingAccountAssetsRepository CreateAccountAssetsRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new MarginTradingAccountAssetsRepository(AzureTableStorage<MarginTradingAccountAssetEntity>.Create(connStringReloadingManager,
                    "MarginTradingAccountAssets", log));
            }

            public static MarginTradingAssetsRepository CreateAssetsRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new MarginTradingAssetsRepository(AzureTableStorage<MarginTradingAssetEntity>.Create(connStringReloadingManager,
                    "MarginTradingAssets", log));
            }

            public static MarginTradingOrdersHistoryRepository CreateOrdersHistoryRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new MarginTradingOrdersHistoryRepository(AzureTableStorage<MarginTradingOrderHistoryEntity>.Create(connStringReloadingManager,
                    "MarginTradingOrdersHistory", log));
            }

            public static MarginTradingOrdersRejectedRepository CreateOrdersRejectedRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new MarginTradingOrdersRejectedRepository(AzureTableStorage<MarginTradingOrderRejectedEntity>.Create(connStringReloadingManager,
                    "MarginTradingOrdersRejected", log));
            }

            public static MarginTradingAccountHistoryRepository CreateAccountHistoryRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new MarginTradingAccountHistoryRepository(AzureTableStorage<MarginTradingAccountHistoryEntity>.Create(connStringReloadingManager,
                    "MarginTradingAccountsHistory", log));
            }

            public static MarginTradingAccountsRepository CreateAccountsRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new MarginTradingAccountsRepository(AzureTableStorage<MarginTradingAccountEntity>.Create(connStringReloadingManager,
                    "MarginTradingAccounts", log));
            }

            public static MarginTradingBlobRepository CreateBlobRepository(IReloadingManager<string> connStringReloadingManager)
            {
                return new MarginTradingBlobRepository(connStringReloadingManager);
            }

            public static MarginTradingWatchListsRepository CreateWatchListsRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new MarginTradingWatchListsRepository(AzureTableStorage<MarginTradingWatchListEntity>.Create(connStringReloadingManager,
                    "MarginTradingWatchLists", log));
            }

            public static MatchingEngineRoutesRepository CreateMatchingEngineRoutesRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new MatchingEngineRoutesRepository(AzureTableStorage<MatchingEngineRouteEntity>.Create(connStringReloadingManager,
                    "MatchingEngineRoutes", log));
            }

            public static AccountsStatsReportsRepository CreateAccountsStatsReportsRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new AccountsStatsReportsRepository(AzureTableStorage<AccountsStatReport>.Create(connStringReloadingManager,
                    "ClientAccountsStatusReports", log));
            }

            public static AccountsReportsRepository CreateAccountsReportsRepository(IReloadingManager<string> connStringReloadingManager, ILog log)
            {
                return new AccountsReportsRepository(AzureTableStorage<AccountsReport>.Create(connStringReloadingManager,
                    "ClientAccountsReports", log));
            }
        }
    }
}