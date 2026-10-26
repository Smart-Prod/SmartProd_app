using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs
{
    public class UpdateOrderStatusDto
    {
        [Required]
        [StringLength(30, MinimumLength = 2)]
        public string Status { get; set; } = string.Empty;
        [Range(0, double.MaxValue)]
        public double? Produced { get; set; }
    }
}