using LugaPasal.Entities;

namespace LugaPasal.Models
{
    public class AnalyticsModel
    {
        public Ratings?LowestRatedProduct { get; set; }
        public Ratings?HighestRatedProduct { get; set; }
        public dynamic ?HighestSellingProduct { get;set; }
        public dynamic?LowestSellingProduct { get; set; }
        public decimal TotalSales { get; set; }
        public List<dynamic> TotalSalesPerProduct { get; set; } = new();
        public List<Products> ?StockLevels { get; set; }
        public int TotalOrders { get; set; }
        public List<dynamic> ?ReviewsPerProduct { get; set; }
        public List<dynamic> ?RatingSummary { get; set; }

        public List<Products>? AllProducts { get; set; }



    }
}
