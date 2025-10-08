using System.ComponentModel.DataAnnotations;

namespace LugaPasal.Models
{
    public class EditProductModel
    {
        public Guid ProductID { get; set; }

        [Required(ErrorMessage = "Product Name is Required")]
        public required string ProductName { get; set; }
        public string? ProductDescription { get; set; }

        [Required(ErrorMessage = "Product Price is Required")]
        public required int ProductPrice { get; set; }

        [Required(ErrorMessage = "Product Quantity is Required")]
        public required int ProductQuantity { get; set; }
        public IFormFile? ProductImage { get; set; }
        public string? ProductImagePath { get; set; }
        public string? ProductCategory { get; set; }
    }
}
