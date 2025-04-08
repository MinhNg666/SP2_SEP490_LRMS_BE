using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Responses
{
    public class DocumentResponse
    {
        public int DocumentId { get; set; }
        public string FileName { get; set; }
        public string DocumentUrl { get; set; }
        public int DocumentType { get; set; }
        public DateTime UploadAt { get; set; }
    }
}
