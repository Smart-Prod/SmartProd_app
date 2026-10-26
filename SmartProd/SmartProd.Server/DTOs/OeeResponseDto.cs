namespace SmartProd.API.Server.DTOs;

public sealed record OeeResponseDto(
    int MachineId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal ScheduledSeconds,
    decimal DowntimeSeconds,
    decimal OperatingSeconds,
    decimal GoodQuantity,
    decimal TotalQuantity,
    decimal AvailabilityPercent,
    decimal PerformancePercent,
    decimal QualityPercent,
    decimal OeePercent);
