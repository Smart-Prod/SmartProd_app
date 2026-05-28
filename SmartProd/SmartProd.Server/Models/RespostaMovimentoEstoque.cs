namespace SmartProd.API.Server.Models
{
    public class RespostaMovimentoEstoque
    {
        public ResumoMovimentacaoEstoque? Resumo { get; set; }
        public int Total { get; set; }
        public int SaldoLiquido { get; set; }
        public List<Movimentacao> Movements { get; set; } = new();
    }
}
