using System.Composition.Convention;

namespace LugaPasal.Models
{
    public class ProductProfileModel
    {
       public required LugaPasal.Entities.Products product { get; set; }
        public required List<LugaPasal.Entities.Products> recommendedProducts { get; set; }
    }
}
 