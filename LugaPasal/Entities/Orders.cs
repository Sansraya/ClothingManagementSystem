using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LugaPasal.Entities
{
    public class Orders
    {
        [Key]
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string ?UserID { get; set; }
        [ForeignKey("UserID")]
        public User User {get; set; }
        public Guid ?ProductID { get; set; }
        [ForeignKey("ProductID")]
        public Products Product { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
