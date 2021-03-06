﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.Common.BackendContracts;
using MarginTrading.Common.Mappers;
using MarginTrading.Core;
using MarginTrading.DataReader.Helpers;
using MarginTrading.DataReader.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.DataReader.Controllers
{
    [Authorize]
    [Route("api/accountHistory")]
    public class AccountHistoryController : Controller
    {
        private readonly IMarginTradingAccountHistoryRepository _accountsHistoryRepository;
        private readonly IMarginTradingOrdersHistoryRepository _ordersHistoryRepository;
        private readonly IOrdersSnapshotReaderService _ordersSnapshotReaderService;
        private readonly IMarginTradingAccountsRepository _accountsRepository;

        public AccountHistoryController(
            IMarginTradingAccountsRepository accountsRepository,
            IMarginTradingAccountHistoryRepository accountsHistoryRepository,
            IMarginTradingOrdersHistoryRepository ordersHistoryRepository,
            IOrdersSnapshotReaderService ordersSnapshotReaderService)
        {
            _accountsRepository = accountsRepository;
            _accountsHistoryRepository = accountsHistoryRepository;
            _ordersHistoryRepository = ordersHistoryRepository;
            _ordersSnapshotReaderService = ordersSnapshotReaderService;
        }

        [Route("byTypes")]
        [HttpGet]
        public async Task<AccountHistoryBackendResponse> GetAccountHistoryByTypes([FromQuery] AccountHistoryBackendRequest request)
        {
            var clientAccountIds = string.IsNullOrEmpty(request.AccountId)
                ? (await _accountsRepository.GetAllAsync(request.ClientId)).Select(item => item.Id).ToArray()
                : new[] {request.AccountId};

            var accounts = (await _accountsHistoryRepository.GetAsync(clientAccountIds, request.From, request.To))
                .Where(item => item.Type != AccountHistoryType.OrderClosed);

            var orders =
                (await _ordersHistoryRepository.GetHistoryAsync(request.ClientId, clientAccountIds, request.From,
                    request.To))
                .Where(item => item.Status != OrderStatus.Rejected);

            var openPositions = await _ordersSnapshotReaderService.GetActiveByAccountIdsAsync(clientAccountIds);

            return new AccountHistoryBackendResponse
            {
                Account = accounts.Select(AccountHistoryExtensions.ToBackendContract)
                    .OrderByDescending(item => item.Date).ToArray(),
                OpenPositions = openPositions.Select(OrderExtensions.ToBackendHistoryContract)
                    .OrderByDescending(item => item.OpenDate).ToArray(),
                PositionsHistory = orders.Select(OrderHistoryExtensions.ToBackendHistoryContract)
                    .OrderByDescending(item => item.OpenDate).ToArray(),
            };
        }

        [Route("byAccounts")]
        [HttpGet]
        public async Task<Dictionary<string, AccountHistoryBackendContract[]>> GetAccountHistoryByAccounts(
            [FromQuery] string accountId = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var accountIds = accountId != null
                ? new[] {accountId}
                : (await _accountsRepository.GetAllAsync()).Select(item => item.Id).ToArray();

            return (await _accountsHistoryRepository.GetAsync(accountIds, from, to)).GroupBy(i => i.AccountId)
                .ToDictionary(g => g.Key, g => g.Select(AccountHistoryExtensions.ToBackendContract).ToArray());
        }

        [Route("timeline")]
        [HttpGet]
        public async Task<AccountNewHistoryBackendResponse> GetAccountHistoryTimeline(
            [FromQuery] AccountHistoryBackendRequest request)
        {
            var clientAccountIds = string.IsNullOrEmpty(request.AccountId)
                ? (await _accountsRepository.GetAllAsync(request.ClientId)).Select(item => item.Id).ToArray()
                : new[] {request.AccountId};

            var accounts = (await _accountsHistoryRepository.GetAsync(clientAccountIds, request.From, request.To))
                .Where(item => item.Type != AccountHistoryType.OrderClosed);

            var openOrders = await _ordersSnapshotReaderService.GetActiveByAccountIdsAsync(clientAccountIds);

            var history = (await _ordersHistoryRepository.GetHistoryAsync(request.ClientId, clientAccountIds,
                    request.From, request.To))
                .Where(item => item.Status != OrderStatus.Rejected)
                .Where(item => item.OpenDate != null).ToList();

            var items = accounts.Select(item => new AccountHistoryItemBackend
                {
                    Account = AccountHistoryExtensions.ToBackendContract(item),
                    Date = item.Date
                })
                .Concat(openOrders.Select(item => new AccountHistoryItemBackend
                {
                    Position = item.ToBackendHistoryContract(),
                    Date = item.OpenDate.Value
                }))
                .Concat(history.Select(item => new AccountHistoryItemBackend
                {
                    Position = OrderHistoryExtensions.ToBackendHistoryOpenedContract(item),
                    Date = item.OpenDate.Value
                }))
                .Concat(history.Select(item => new AccountHistoryItemBackend
                {
                    Position = OrderHistoryExtensions.ToBackendHistoryContract(item),
                    Date = item.CloseDate.Value
                }))
                .OrderByDescending(item => item.Date);

            return new AccountNewHistoryBackendResponse
            {
                HistoryItems = items.ToArray()
            };
        }
    }
}