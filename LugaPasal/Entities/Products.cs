using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LugaPasal.Entities
{
    public class Products
    {
        [Key]
        public required Guid ProductID {get; set; }

        [Required(ErrorMessage = "Product Name is Required")]
        public required string ProductName { get; set; }

        public string ?ProductDescription { get; set; }

        [Required(ErrorMessage = "Product Price is Required")]
        public int ProductPrice { get; set; } 

        [Required(ErrorMessage = "Product Quantity is Required")]
        public int ProductQuantity { get; set; }
        public string? ProductCategory { get; set; }
        public string ?ProductImagePath { get; set; }
        public string UserId { get; set; } =string.Empty;
        public User User { get; set; } = null!;
        
        public ICollection<Orders>?Orders { get; set; }
        public ICollection<Ratings>? Ratings { get; set; }
    }
}
