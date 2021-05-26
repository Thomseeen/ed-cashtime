using System;

namespace EdCashtime.Models {
    public abstract class SearchCommodityModel {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public virtual bool TestLimits(Commodity com) => throw new NotImplementedException();

        public virtual string AlertString(Message metadata, Commodity com) => throw new NotImplementedException();


    }

    public class BuyCommodityModel : SearchCommodityModel {
        public long MinPrice { get; set; }

        public long MinDemand { get; set; }

        public override bool TestLimits(Commodity com) {
            return com.BuyPrice >= MinPrice && com.Demand > MinDemand;
        }

        public override string AlertString(Message metadata, Commodity com) {
            return $"Buying {DisplayName} @{metadata.SystemName} - {metadata.StationName}: {com.Demand}x {com.BuyPrice}";
        }
    }

    public class SellCommodityModel : SearchCommodityModel {
        public long MaxPrice { get; set; }

        public long MinStock { get; set; }

        public long BestPrice { get; set; }

        public long BestStock { get; set; }

        public override bool TestLimits(Commodity com) {
            return com.SellPrice <= MaxPrice && com.Stock > MinStock;
        }

        public override string AlertString(Message metadata, Commodity com) {
            return $"Selling {DisplayName} @{metadata.SystemName} - {metadata.StationName}: {com.Stock}x {com.SellPrice}";
        }
    }
}