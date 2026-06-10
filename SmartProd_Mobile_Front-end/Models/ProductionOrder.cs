using System;
using System.Collections.Generic;
using System.Text;

namespace SmartProd_Mobile_Front_end.Models
{
    public class ProductionOrder
    {
        public string? Id { get; set; }
        public string? Product { get; set; }
        public string? Status { get; set; }
        public string? CreatedAt { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? StatusColor { get; set; }
    }
}
