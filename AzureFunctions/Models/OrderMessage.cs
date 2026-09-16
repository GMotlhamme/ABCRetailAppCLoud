using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctions.Models
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
