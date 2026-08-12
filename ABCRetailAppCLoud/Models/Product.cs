using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABCRetailAppCLoud.Models
{
    public class Product : ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        [Required]
        public string ProductName { get; set; }
        public double Price { get; set; }
        [Required]
        public string Description { get; set; }
        public string? ProductImage { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
