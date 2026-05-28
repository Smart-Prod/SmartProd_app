namespace SmartProd.API.Server.DTOs
{
    public class OrdemProducaoCreateDto
    {
        public int ProductId { get; set; }
        public int UsuarioId { get; set; }
        public double Quantity { get; set; }
    }
}
