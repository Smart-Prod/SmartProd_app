namespace SmartProd.API.Server.DTOs;

public sealed class BomResponseDto
{
    public int Id { get; init; }
    public int ProdutoId { get; init; }
    public ProdutoSummaryDto? Produto { get; init; }
    public List<BomItemResponseDto> Materials { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class BomItemResponseDto
{
    public int Id { get; init; }
    public int MateriaisId { get; init; }
    public int ProdutosId { get; init; }
    public double Quantidade { get; init; }
    public ProdutoSummaryDto? Produtos { get; init; }
}

public sealed class ProdutoSummaryDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public double EstoqueAtual { get; init; }
    public double EstoqueReservado { get; init; }
    public double EstoqueMínimo { get; init; }
    public int UsuarioId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
