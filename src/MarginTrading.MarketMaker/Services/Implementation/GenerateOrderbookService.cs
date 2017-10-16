﻿using System;
using System.Collections.Immutable;
using System.Linq;
using Autofac;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.HelperServices;
using MarginTrading.MarketMaker.HelperServices.Implemetation;
using MarginTrading.MarketMaker.Models;

namespace MarginTrading.MarketMaker.Services.Implementation
{
    /// <summary>
    ///     Generates orderbooks from external exchanges
    /// </summary>
    /// <remarks>
    ///     https://lykkex.atlassian.net/wiki/spaces/MW/pages/84607035/Price+setting
    /// </remarks>
    public class GenerateOrderbookService : IStartable, IDisposable
    {
        private readonly IOrderbooksService _orderbooksService;
        private readonly IDisabledOrderbooksService _disabledOrderbooksService;
        private readonly ISystem _system;
        private readonly IOutdatedOrderbooksService _outdatedOrderbooksService;
        private readonly IOutliersOrderbooksService _outliersOrderbooksService;
        private readonly IRepeatedProblemsOrderbooksService _repeatedProblemsOrderbooksService;
        private readonly IPriceCalcSettingsService _priceCalcSettingsService;
        private readonly IAlertService _alertService;
        private readonly IPrimaryExchangeService _primaryExchangeService;
        private readonly IArbitrageFreeSpreadService _arbitrageFreeSpreadService;
        private readonly IBestPricesService _bestPricesService;


        public GenerateOrderbookService(
            IOrderbooksService orderbooksService,
            IDisabledOrderbooksService disabledOrderbooksService,
            ISystem system,
            IOutdatedOrderbooksService outdatedOrderbooksService,
            IOutliersOrderbooksService outliersOrderbooksService,
            IRepeatedProblemsOrderbooksService repeatedProblemsOrderbooksService,
            IPriceCalcSettingsService priceCalcSettingsService,
            IAlertService alertService,
            IPrimaryExchangeService primaryExchangeService,
            IArbitrageFreeSpreadService arbitrageFreeSpreadService,
            IBestPricesService bestPricesService)
        {
            _orderbooksService = orderbooksService;
            _disabledOrderbooksService = disabledOrderbooksService;
            _system = system;
            _outdatedOrderbooksService = outdatedOrderbooksService;
            _outliersOrderbooksService = outliersOrderbooksService;
            _repeatedProblemsOrderbooksService = repeatedProblemsOrderbooksService;
            _priceCalcSettingsService = priceCalcSettingsService;
            _alertService = alertService;
            _primaryExchangeService = primaryExchangeService;
            _arbitrageFreeSpreadService = arbitrageFreeSpreadService;
            _bestPricesService = bestPricesService;
        }

        [CanBeNull]
        public Orderbook OnNewOrderbook(ExternalOrderbook orderbook)
        {
            var assetPairId = orderbook.AssetPairId;
            var allOrderbooks = _orderbooksService.AddAndGetByAssetPair(orderbook);
            var (exchangesErrors, validOrderbooks) = MarkExchangesErrors(assetPairId, allOrderbooks);
            var primaryExchange = _primaryExchangeService.GetPrimaryExchange(assetPairId, exchangesErrors);
            if (primaryExchange == null)
            {
                return null;
            }

            return Transform(allOrderbooks[primaryExchange], validOrderbooks);
        }

        /// <summary>
        ///     Detects exchanges errors and disables thm if they get repeated
        /// </summary>
        private (ImmutableDictionary<string, ExchangeErrorState>, ImmutableDictionary<string, ExternalOrderbook>)
            MarkExchangesErrors(string assetPairId, ImmutableDictionary<string, ExternalOrderbook> allOrderbooks)
        {
            var now = _system.UtcNow;
            var disabledExchanges = _disabledOrderbooksService.GetDisabledExchanges(assetPairId);
            var enabledOrderbooks = allOrderbooks.RemoveRange(disabledExchanges);
            var (outdatedExchanges, upToDateOrderbooks) = FindOutdated(assetPairId, enabledOrderbooks, now);
            var (outliersExchanges, validOrderbooks) = FindOutliers(assetPairId, upToDateOrderbooks);

            var repeatedProblemsExchanges = GetRepeatedProblemsExchanges(assetPairId, enabledOrderbooks,
                outdatedExchanges, outliersExchanges, now);
            _disabledOrderbooksService.Disable(assetPairId, repeatedProblemsExchanges);

            var exchangesErrors = ImmutableDictionary.CreateBuilder<string, ExchangeErrorState>()
                .SetValueForKeys(disabledExchanges, ExchangeErrorState.Disabled)
                .SetValueForKeys(outdatedExchanges, ExchangeErrorState.Outdated)
                .SetValueForKeys(outliersExchanges, ExchangeErrorState.Outlier)
                .SetValueForKeys(validOrderbooks.Keys, ExchangeErrorState.None)
                .SetValueForKeys(repeatedProblemsExchanges, ExchangeErrorState.Disabled)
                .ToImmutable();

            return (exchangesErrors, validOrderbooks);
        }

        /// <summary>
        ///     Applies arbitrage-free spread to the orderbook
        /// </summary>
        [CanBeNull]
        private Orderbook Transform(
            ExternalOrderbook primaryOrderbook,
            ImmutableDictionary<string, ExternalOrderbook> upToDateNotoutlierOrderbooks)
        {
            if (!_priceCalcSettingsService.IsStepEnabled(OrderbookGeneratorStepEnum.Transform,
                primaryOrderbook.AssetPairId))
            {
                return primaryOrderbook;
            }

            var bestPrices =
                upToDateNotoutlierOrderbooks.Values.ToDictionary(o => o.ExchangeName, _bestPricesService.Calc);
            return _arbitrageFreeSpreadService.Transform(primaryOrderbook, bestPrices);
        }

        /// <summary>
        ///     Detects exchanges with repeated problems
        /// </summary>
        private ImmutableHashSet<string> GetRepeatedProblemsExchanges(string assetPairId,
            ImmutableDictionary<string, ExternalOrderbook> orderbooksByExchanges,
            ImmutableHashSet<string> outdatedExchanges, ImmutableHashSet<string> outliersExchanges,
            DateTime now)
        {
            if (!_priceCalcSettingsService.IsStepEnabled(OrderbookGeneratorStepEnum.FindRepeatedProblems, assetPairId))
            {
                return ImmutableHashSet<string>.Empty;
            }

            return orderbooksByExchanges.Values
                .Where(o => _repeatedProblemsOrderbooksService.IsRepeatedProblemsOrderbook(o,
                    outdatedExchanges.Contains(o.ExchangeName), outliersExchanges.Contains(o.ExchangeName), now))
                .Select(o => o.ExchangeName).ToImmutableHashSet();
        }


        /// <summary>
        ///     Detects outlier exchanges
        /// </summary>
        private (ImmutableHashSet<string>, ImmutableDictionary<string, ExternalOrderbook>) FindOutliers(
            string assetPairId, ImmutableDictionary<string, ExternalOrderbook> upToDateOrderbooks)
        {
            if (!_priceCalcSettingsService.IsStepEnabled(OrderbookGeneratorStepEnum.FindOutliers, assetPairId))
            {
                return (ImmutableHashSet<string>.Empty, upToDateOrderbooks);
            }

            if (upToDateOrderbooks.Count < 3)
            {
                _alertService.AlertStopNewTrades(assetPairId);
                return (ImmutableHashSet<string>.Empty, upToDateOrderbooks);
            }

            var outliersExchanges = _outliersOrderbooksService.FindOutliers(upToDateOrderbooks)
                .Select(o => o.ExchangeName)
                .ToImmutableHashSet();
            var upToDateNotoutlierOrderbooks = upToDateOrderbooks.RemoveRange(outliersExchanges);
            return (outliersExchanges, upToDateNotoutlierOrderbooks);
        }


        /// <summary>
        ///     Detects outdates exchanges
        /// </summary>
        private (ImmutableHashSet<string>, ImmutableDictionary<string, ExternalOrderbook>)
            FindOutdated(string assetPairId, ImmutableDictionary<string, ExternalOrderbook> orderbooksByExchanges,
                DateTime now)
        {
            if (!_priceCalcSettingsService.IsStepEnabled(OrderbookGeneratorStepEnum.FindOutdated, assetPairId))
            {
                return (ImmutableHashSet<string>.Empty, orderbooksByExchanges);
            }

            var outdatedExchanges = orderbooksByExchanges.Values
                .Where(o => _outdatedOrderbooksService.IsOutdated(o, now)).Select(o => o.ExchangeName)
                .ToImmutableHashSet();
            var upToDateOrderbooks = orderbooksByExchanges.RemoveRange(outdatedExchanges);
            return (outdatedExchanges, upToDateOrderbooks);
        }

        public void Start()
        {
            _alertService.AlertStarted();
        }

        public void Dispose()
        {
            _alertService.AlertStopping().GetAwaiter().GetResult();
        }
    }
}