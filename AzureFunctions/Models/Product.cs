using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctions.Models
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
