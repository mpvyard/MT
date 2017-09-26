using System;
using System.Collections.Generic;
using MarginTrading.Core;

namespace MarginTrading.Common.BackendContracts
{
    public class OrderFullContract : OrderContract
    {
        public string TradingConditionId { get; set; }
        public double QuoteRate { get; set; }
        public int AssetAccuracy { get; set; }
        public double CommissionLot { get; set; }
        public DateTime? StartClosingDate { get; set; }
        public OrderFillType FillType { get; set; }
        public string Comment { get; set; }
        public double InterestRateSwap { get; set; }
        public double MarginInit { get; set; }
        public double MarginMaintenance { get; set; }
        public double OpenCrossPrice { get; set; }
        public double CloseCrossPrice { get; set; }
    }
}
