using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.Core.Settings;

namespace MarginTrading.AccountHistoryBroker.AzureRepositories
{
    internal class AccountTransactionsReportsRepository : IAccountTransactionsReportsRepository
    {
        private readonly INoSQLTableStorage<AccountTransactionsReportsEntity> _tableStorage;

        public AccountTransactionsReportsRepository(IReloadingManager<MarginSettings> settings, ILog log)
        {
            _tableStorage = AzureTableStorage<AccountTransactionsReportsEntity>.Create(settings.Nested(s => s.Db.ReportsConnString),
                "MarginTradingAccountTransactionsReports", log);
        }

        public Task InsertOrReplaceAsync(AccountTransactionsReportsEntity entity)
        {
            return _tableStorage.InsertOrReplaceAsync(entity);
        }
    }
}
