namespace EdCashtime.Models {
    public class SearchCommodityModel {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public long? MinBuyPrice { get; set; }

        public long? MinBuyDemand { get; set; }

        public long? MaxSellPrice { get; set; }

        public long? MinStock { get; set; }
    }
}