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
        public List<ProductSalesViewModel> TotalSalesPerProduct { get; set; } = new();
        public List<ProductSalesViewModel> TotalSalesPerProductQuantity { get; set; } = new();
        public List<Products> ?StockLevels { get; set; }
        public int TotalOrders { get; set; }
        public List<ProductReviewViewModel> ?ReviewsPerProduct { get; set; }
        public List<ProductRatingSummaryViewModel> ?RatingSummary { get; set; }

        public List<Products>? AllProducts { get; set; }



    }
    public class ProductReviewViewModel
    {
        public Guid ProductID { get; set; }
        public int TotalReviews { get; set; }
    }
    public class ProductRatingSummaryViewModel
    {
        public Guid ProductId { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int OneStarCount { get; set; }
        public double FiveStarPercent { get; set; }
        public double OneStarPercent { get; set; }
    }
}
