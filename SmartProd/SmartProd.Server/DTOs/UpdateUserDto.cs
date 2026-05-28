using SmartProd.API.Server.Enum;

namespace SmartProd.API.Server.DTOs
{
    public class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public UserRole? Role { get; set; }
        public bool? Active { get; set; }
    }
}