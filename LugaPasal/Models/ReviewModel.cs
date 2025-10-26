

namespace LugaPasal.Models
{
    public class ReviewModel
    {
        public LugaPasal.Entities.Products product { get; set; }
        public required int RatingValue { get; set; }
        public required string Review { get; set; }
        public required string  UserID { get; set; }
    }
}
