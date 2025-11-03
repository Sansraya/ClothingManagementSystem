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
        public User User { get; set; }
        public required string OrderStatus { get; set; } = "Pending";
        public decimal OrderTotalPrice { get; set; }
        public ICollection<OrderItems> Items { get; set; } = new List<OrderItems>();
    }

    public class OrderItems
    {
        [Key]
        public Guid OrderItemsId { get; set; }
        public Guid OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Orders Order { get; set; }
        public Guid ProductID { get; set; }
        [ForeignKey("ProductID")]
        public Products Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
