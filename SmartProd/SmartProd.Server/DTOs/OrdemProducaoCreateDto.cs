using System.ComponentModel.DataAnnotations;

namespace SmartProd.API.Server.DTOs
{
    public class OrdemProducaoCreateDto
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        [Range(1, int.MaxValue)]
        public int UsuarioId { get; set; }
        [Range(double.Epsilon, double.MaxValue)]
        public double Quantity { get; set; }
    }
}
