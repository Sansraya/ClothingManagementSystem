using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LugaPasal.Entities
{
    public class Ratings
    {
        [Key]
        public required Guid RatingID { get; set; }
        public int RatingValue { get; set; }
        public string? Review { get; set; }
        public Guid ?ProductID { get; set; }
        [ForeignKey("ProductID")]
        public Products Product { get; set; } 
        public string ?UserID { get; set; } = string.Empty;
        [ForeignKey("UserID")]
        public User User { get; set; } 


    }
}
