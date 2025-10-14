using LugaPasal.Entities;

namespace LugaPasal.Models
{
    public class DashboardModel
    {
        public int totalUser { get; set; }
        public int totalProducts { get; set; }
        public int highRatedProducts { get; set; }
        public required Dictionary<string,int> categoryCounts { get; set; }
        public int outOfStock { get; set; }
        public double averageRating { get; set; }
        public int lowRated { get; set; }

        public int TotalReviews { get; set; }
        public required List<Products> expensiveProducts { get; set; }
        public double averagePrice { get; set; }
        public int lowStock { get; set; }
        public Products topRatedProduct { get; set; } = null;
        public Products lowRatedProduct { get; set; }
        public int productWithReviews { get; set; }
        public double percentageReview { get; set; }


    }
}
