using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using MarginTrading.Core.Settings;
using MarginTrading.Core;

namespace MarginTrading.AccountHistoryBroker.Repositories.AzureRepositories
{
    internal class AccountTransactionsReportsRepository : IAccountTransactionsReportsRepository
    {
        private readonly INoSQLTableStorage<AccountTransactionsReportsEntity> _tableStorage;

        public AccountTransactionsReportsRepository(MarginSettings settings, ILog log)
        {
            _tableStorage = AzureTableStorage<AccountTransactionsReportsEntity>.Create(() => settings.Db.ReportsConnString,
                "MarginTradingAccountTransactionsReports", log);
        }

        public Task InsertOrReplaceAsync(IAccountTransactionsReport entity)
        {
            return _tableStorage.InsertOrReplaceAsync(AccountTransactionsReportsEntity.Create(entity));
        }
    }
}
