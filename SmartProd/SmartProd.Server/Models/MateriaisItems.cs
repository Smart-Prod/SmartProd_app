namespace SmartProd.API.Server.Models
{
    public class MateriaisItems
    {
        public int Id { get; set; }
        public int MateriaisId { get; set; }
        public Materiais Materiais { get; set; } = null!;

        public int ProdutosId { get; set; }
        public Produto Produtos { get; set; } = null!;

        public double Quantidade { get; set; }
    }
}
