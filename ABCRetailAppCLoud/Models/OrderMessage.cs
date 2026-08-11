namespace ABCRetailAppCLoud.Models
{
    public class OrderMessage
    {
        public string? OrderId { get; set; }
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
        public string Action { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
