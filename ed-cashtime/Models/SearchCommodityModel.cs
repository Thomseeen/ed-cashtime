using System;

namespace EdCashtime.Models {
    public abstract class SearchCommodityModel {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public virtual bool TestLimits(Commodity com) => throw new NotImplementedException();

        public virtual string AlertString(Message metadata, Commodity com) => throw new NotImplementedException();

        public virtual bool CompareComs(Commodity oldcom, Commodity newcom) => throw new NotImplementedException();
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

        public override bool CompareComs(Commodity oldcom, Commodity newcom) {
            return newcom.BuyPrice > oldcom.BuyPrice && (newcom.Demand > oldcom.Demand || newcom.Demand > MinDemand);
        }
    }

    public class SellCommodityModel : SearchCommodityModel {
        public long MaxPrice { get; set; }

        public long MinStock { get; set; }

        public override bool TestLimits(Commodity com) {
            return com.SellPrice <= MaxPrice && com.Stock > MinStock;
        }

        public override string AlertString(Message metadata, Commodity com) {
            return $"Selling {DisplayName} @{metadata.SystemName} - {metadata.StationName}: {com.Stock}x {com.SellPrice}";
        }

        public override bool CompareComs(Commodity oldcom, Commodity newcom) {
            return newcom.SellPrice < oldcom.SellPrice && (newcom.Stock > oldcom.Stock || newcom.Stock > MinStock);
        }
    }
}