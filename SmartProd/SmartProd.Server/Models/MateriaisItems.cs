namespace SmartProd.API.Server.Models
{
    public class MateriaisItems
    {
        public int Id { get; set; }
        public int MateriasId { get; set; }
        public Materiais Materiais { get; set; } = null!;

        public int MaterialId { get; set; }
        public Produto Produtos { get; set; } = null!;

        public double Quantidade { get; set; }
    }
}
