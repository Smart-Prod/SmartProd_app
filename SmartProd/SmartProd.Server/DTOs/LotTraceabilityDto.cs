namespace SmartProd.API.Server.DTOs;

public sealed record LotTraceabilityDto(
    int LotId,
    string LotNumber,
    string ProductName,
    string? SupplierLotNumber,
    decimal QuantityReceived,
    decimal QuantityRemaining,
    DateTime? ExpiresAt,
    int? ProductionOrderId,
    IReadOnlyList<LotGenealogyEdgeDto> Inputs,
    IReadOnlyList<LotGenealogyEdgeDto> Outputs);

public sealed record LotGenealogyEdgeDto(
    int InputLotId,
    string InputLotNumber,
    string InputProductName,
    int OutputLotId,
    string OutputLotNumber,
    string OutputProductName,
    int ProductionOrderId,
    decimal QuantityConsumed,
    DateTime RecordedAt);
