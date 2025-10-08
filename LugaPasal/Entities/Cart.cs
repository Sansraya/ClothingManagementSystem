namespace LugaPasal.Entities
{
    public class Cart
    {
        public Guid ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }

        public string? ProductImagePath { get; set; }
    }
}
