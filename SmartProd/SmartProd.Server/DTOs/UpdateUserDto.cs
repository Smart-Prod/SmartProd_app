using SmartProd.API.Server.Enum;
using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs
{
    public class UpdateUserDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }
        [EmailAddress]
        [StringLength(254)]
        public string? Email { get; set; }
        public UserRole? Role { get; set; }
        public bool? Active { get; set; }
    }
}