﻿using System;
using Autofac;
using MarginTrading.Core;
using MarginTrading.DataReader.Middleware.Validator;
using MarginTrading.DataReader.Services;
using MarginTrading.DataReader.Services.Implementation;
using MarginTrading.Services;
using Rocks.Caching;

namespace MarginTrading.DataReader.Modules
{
    public class DataReaderServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ApiKeyValidator>().As<IApiKeyValidator>()
                .SingleInstance();
            builder.RegisterType<OrderBookSnapshotReaderService>().As<IOrderBookSnapshotReaderService>()
                .SingleInstance();
            builder.RegisterType<OrdersSnapshotReaderService>().As<IOrdersSnapshotReaderService>()
                .SingleInstance();
            builder.RegisterType<AccountAssetsCacheService>().As<IAccountAssetsCacheService>()
                .SingleInstance();
            builder.RegisterType<MarginTradingSettingsService>().As<IMarginTradingSettingsService>()
                .SingleInstance();
            builder.RegisterType<MemoryCacheProvider>().As<ICacheProvider>()
                .SingleInstance();

        }
    }
}