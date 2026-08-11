using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABCRetailAppCLoud.Models
{
    
        public class Customer : ITableEntity
        {
            public string? PartitionKey { get; set; }
            public string? RowKey { get; set; }

            [Required]
            public string FullName { get; set; }

            [Required]
            public string Email { get; set; }

            public string Address { get; set; }

            public string City { get; set; }

            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }
        }
    
}
