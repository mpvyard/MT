﻿using System;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.Common.Extensions;
using MarginTrading.Core;
using MarginTrading.Core.MarketMakerFeed;

namespace MarginTrading.Services
{
    public class MarketMakerService : IFeedConsumer
    {
        private readonly IMatchingEngine _matchingEngine;

        public MarketMakerService(IMatchingEngine matchingEngine)
        {
            _matchingEngine = matchingEngine;
        }

        public void ConsumeFeed(MarketMakerOrderCommandsBatchMessage batch)
        {
            batch.AssetPairId.RequiredNotNullOrWhiteSpace(nameof(batch.AssetPairId));
            batch.Commands.RequiredNotNull(nameof(batch.Commands));

            var model = new SetOrderModel {MarketMakerId = batch.MarketMakerId};

            ConvertCommandsToOrders(batch, model);
            if (model.OrdersToAdd?.Count > 0 || model.DeleteByInstrumentsBuy?.Count > 0 ||
                model.DeleteByInstrumentsSell?.Count > 0)
            {
                _matchingEngine.SetOrders(model);
            }
        }

        private void ConvertCommandsToOrders(MarketMakerOrderCommandsBatchMessage batch, SetOrderModel model)
        {
            var setCommands = batch.Commands.Where(c => c.CommandType == MarketMakerOrderCommandType.SetOrder && c.Direction != null).ToList();
            model.OrdersToAdd = setCommands
                .Select(c => new LimitOrder
                {
                    Id = Guid.NewGuid().ToString("N"),
                    MarketMakerId = batch.MarketMakerId,
                    CreateDate = DateTime.UtcNow,
                    Instrument = batch.AssetPairId,
                    Price = c.Price.RequiredNotNull(nameof(c.Price)),
                    Volume = c.Direction.RequiredNotNull(nameof(c.Direction)) == OrderDirection.Buy
                        ? c.Volume.Value
                        : -c.Volume.Value
                }).ToList();

            AddOrdersToDelete(batch, model);
        }

        private static void AddOrdersToDelete(MarketMakerOrderCommandsBatchMessage batch, SetOrderModel model)
        {
            var directions = batch.Commands.Where(c => c.CommandType == MarketMakerOrderCommandType.DeleteOrder).Select(c => c.Direction).Distinct().ToList();
            model.DeleteByInstrumentsBuy = directions.Where(d => d == OrderDirection.Buy || d == null).Select(d => batch.AssetPairId).ToList();
            model.DeleteByInstrumentsSell = directions.Where(d => d == OrderDirection.Sell || d == null).Select(d => batch.AssetPairId).ToList();
        }
    }
}