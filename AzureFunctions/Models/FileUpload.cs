using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctions.Models
{
    public class FileUpload
    {
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}
