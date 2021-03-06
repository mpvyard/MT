﻿using System;
using System.Collections.Generic;
using System.Linq;
using MarginTrading.Common.Mappers;
using MarginTrading.Core;

namespace MarginTrading.Common.BackendContracts
{
    public class InitDataBackendResponse
    {
        public MarginTradingAccountBackendContract[] Accounts { get; set; }
        public Dictionary<string, AccountAssetPairBackendContract[]> AccountAssetPairs { get; set; }
        public bool IsLive { get; set; }

        public static InitDataBackendResponse Create(IEnumerable<IMarginTradingAccount> accounts,
            Dictionary<string, IAccountAssetPair[]> accountAssetPairs, bool isLive)
        {
            return new InitDataBackendResponse
            {
                Accounts = accounts.Select(item => item.ToBackendContract(isLive)).ToArray(),
                AccountAssetPairs = accountAssetPairs.ToDictionary(pair => pair.Key, pair => pair.Value.Select(item => item.ToBackendContract()).ToArray()),
            };
        }

        public static InitDataBackendResponse CreateEmpty()
        {
            return new InitDataBackendResponse
            {
                Accounts = Array.Empty<MarginTradingAccountBackendContract>(),
                AccountAssetPairs = new Dictionary<string, AccountAssetPairBackendContract[]>(),
            };
        }
    }
}
