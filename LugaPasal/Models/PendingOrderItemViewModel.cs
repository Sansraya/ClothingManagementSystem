using LugaPasal.Entities;

namespace LugaPasal.Models
{
    public class PendingOrderItemViewModel
    {
        public Orders Order { get; set; } = null!;
        public OrderItems Item { get; set; } = null!;
    }

}
