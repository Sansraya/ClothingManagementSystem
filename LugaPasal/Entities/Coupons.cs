using System.ComponentModel.DataAnnotations;

namespace LugaPasal.Entities
{
    public class Coupons
    {
        [Key]
        public Guid CouponID { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public DateTime ExpiryDate { get; set; }

    }
}
