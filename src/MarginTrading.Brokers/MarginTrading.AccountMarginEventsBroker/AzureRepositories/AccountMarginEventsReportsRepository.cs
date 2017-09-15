using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.Core.Settings;

namespace MarginTrading.AccountMarginEventsBroker.AzureRepositories
{
    internal class AccountMarginEventsReportsRepository : IAccountMarginEventsReportsRepository
    {
        private readonly INoSQLTableStorage<AccountMarginEventReportEntity> _tableStorage;

        public AccountMarginEventsReportsRepository(IReloadingManager<MarginSettings> settings, ILog log)
        {
            _tableStorage = AzureTableStorage<AccountMarginEventReportEntity>.Create(settings.Nested(s => s.Db.ReportsConnString),
                "AccountMarginEventsReports", log);
        }

        public Task InsertOrReplaceAsync(AccountMarginEventReportEntity entity)
        {
            return _tableStorage.InsertOrReplaceAsync(entity);
        }
    }
}
