using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;

namespace MarginTrading.AzureRepositories.Reports
{
    public class AccountsReport : TableEntity
    {
        public string TakerCounterpartyId
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        public string TakerAccountId
        {
            get => RowKey;
            set => RowKey = value;
        }

        public string BaseAssetId { get; set; }
        public bool IsLive { get; set; }
    }

    public interface IAccountsReportsRepository
    {
        Task InsertOrReplaceBatchAsync(IEnumerable<AccountsReport> stats);
    }

    public class AccountsReportsRepository : IAccountsReportsRepository
    {
        private readonly INoSQLTableStorage<AccountsReport> _tableStorage;

        public AccountsReportsRepository(INoSQLTableStorage<AccountsReport> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task InsertOrReplaceBatchAsync(IEnumerable<AccountsReport> stats)
        {
            return _tableStorage.InsertOrReplaceBatchAsync(stats);
        }
    }
}
