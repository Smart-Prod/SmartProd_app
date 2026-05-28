using SmartProd.API.Server.Enum;
using System.Runtime.ConstrainedExecution;

namespace SmartProd.API.Server.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Active { get; set; }
        public string? Department { get; set; }

        // O tipo aqui deve ser o Enum, não a classe de extensão
        public UserRole Role { get; set; }

        // Propriedades de Navegação
        public List<Produto> Produtos { get; set; } = new();
        public List<OrdemProducao> OrdemProducao { get; set; } = new();
        public List<NotaFiscal> NotaFiscal { get; set; } = new();
    }
}
