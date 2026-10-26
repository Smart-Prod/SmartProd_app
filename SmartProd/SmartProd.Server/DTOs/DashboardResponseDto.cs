namespace SmartProd.API.Server.DTOs;

public sealed class DashboardResponseDto
{
    public int TotalOrders { get; init; }
    public int ActiveOrders { get; init; }
    public int CompletedOrders { get; init; }
    public int TotalMovements { get; init; }
    public int TotalProducts { get; init; }
    public int CriticalStockCount { get; init; }
    public double PlannedQuantity { get; init; }
    public double ProducedQuantity { get; init; }
    public double ProductionEfficiencyPercent { get; init; }
    public decimal Revenue { get; init; }
    public IReadOnlyList<CriticalStockProductDto> CriticalStockProducts { get; init; } = [];
    public IReadOnlyList<RecentMovementDto> RecentMovements { get; init; } = [];
    public IReadOnlyList<CompletedOrderHistoryDto> CompletedOrderHistory { get; init; } = [];
}

public sealed record CriticalStockProductDto(
    int ProductId,
    string Code,
    string Name,
    double CurrentStock,
    double MinimumStock);

public sealed record RecentMovementDto(
    int MovementId,
    int ProductId,
    string ProductName,
    string Type,
    double Quantity,
    DateTime CreatedAt);

public sealed record CompletedOrderHistoryDto(
    int OrderId,
    DateTime FinishedAt,
    double PlannedQuantity,
    double ProducedQuantity,
    string Status);
