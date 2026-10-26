using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Models;
using SmartProd.API.Server.Services;
using SmartProd.API.Server.Utils;
using Xunit;

namespace SmartProd.Tests.Unit;

public sealed class IndustrialOperationsServiceTests
{
    [Fact]
    public async Task RecordEventAsync_IsIdempotentAndRejectsChangedPayload()
    {
        await using var context = CreateContext();
        var order = await CreateOrderAsync(context);
        var service = new IndustrialOperationsService(context);
        var dto = CreateProductionEvent(order.Id, Guid.NewGuid());

        var (recorded, duplicate) = await service.RecordEventAsync(dto, operatorId: null, CancellationToken.None);
        var (replayed, replayedDuplicate) = await service.RecordEventAsync(dto, operatorId: null, CancellationToken.None);

        Assert.False(duplicate);
        Assert.True(replayedDuplicate);
        Assert.Equal(recorded.Id, replayed.Id);
        Assert.Equal(1, (await context.OrdensProducao.SingleAsync()).Produced);
        Assert.Single(await context.ProductionEvents.ToListAsync());

        await Assert.ThrowsAsync<ConflictException>(() => service.RecordEventAsync(
            CreateProductionEvent(order.Id, dto.EventId, goodQuantity: 2),
            operatorId: null,
            CancellationToken.None));
    }

    [Fact]
    public async Task ConsumeLotAsync_UpdatesLotGenealogyAndBothInventoryRepresentations()
    {
        await using var context = CreateContext();
        var rawMaterial = CreateProduct("RAW-001", TipoProduto.MP);
        var finishedGood = CreateProduct("FIN-001", TipoProduto.PA);
        context.Produtos.AddRange(rawMaterial, finishedGood);
        await context.SaveChangesAsync();
        var order = new OrdemProducao
        {
            ProductId = finishedGood.Id,
            UsuarioId = 1,
            Quantity = 5
        };
        context.OrdensProducao.Add(order);
        await context.SaveChangesAsync();
        var service = new IndustrialOperationsService(context);
        var input = await service.CreateLotAsync(new LotCreateDto
        {
            ProductId = rawMaterial.Id,
            Number = "RAW-LOT-001",
            Kind = LotKind.RawMaterial,
            Quantity = 5
        }, CancellationToken.None);
        var output = await service.CreateLotAsync(new LotCreateDto
        {
            ProductId = finishedGood.Id,
            Number = "FIN-LOT-001",
            Kind = LotKind.FinishedGood,
            Quantity = 5,
            ProductionOrderId = order.Id
        }, CancellationToken.None);

        await service.ConsumeLotAsync(new LotConsumptionDto
        {
            InputLotId = input.Id,
            OutputLotId = output.Id,
            ProductionOrderId = order.Id,
            Quantity = 2
        }, CancellationToken.None);

        Assert.Equal(3, (await context.Lots.SingleAsync(lot => lot.Id == input.Id)).QuantityRemaining);
        Assert.Equal(3, (await context.Produtos.SingleAsync(product => product.Id == rawMaterial.Id)).EstoqueAtual);
        Assert.Equal(3, (await context.Estoque.SingleAsync(stock => stock.ProductId == rawMaterial.Id)).Quantity);
        Assert.Single(await context.LotGenealogy.ToListAsync());
        var traceability = await service.GetLotTraceabilityAsync(output.Id, CancellationToken.None);
        Assert.Single(traceability.Inputs);
        Assert.Equal(input.Id, traceability.Inputs[0].InputLotId);

        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            service.ConsumeLotAsync(new LotConsumptionDto
            {
                InputLotId = input.Id,
                OutputLotId = output.Id,
                ProductionOrderId = order.Id,
                Quantity = 0
            }, CancellationToken.None));
    }

    [Fact]
    public async Task CalculateOeeAsync_UsesPriorMachineStateAndPeriodEvents()
    {
        await using var context = CreateContext();
        var product = CreateProduct("FIN-002", TipoProduto.PA);
        var center = new WorkCenter { Code = "WC-001", Name = "Pressing" };
        context.Produtos.Add(product);
        context.WorkCenters.Add(center);
        await context.SaveChangesAsync();
        var machine = new Machine
        {
            WorkCenterId = center.Id,
            Code = "M-001",
            Name = "Press"
        };
        var order = new OrdemProducao { ProductId = product.Id, UsuarioId = 1, Quantity = 100 };
        context.Machines.Add(machine);
        context.OrdensProducao.Add(order);
        await context.SaveChangesAsync();
        var operation = new ProductionOperation
        {
            ProductId = product.Id,
            WorkCenterId = center.Id,
            Sequence = 1,
            Name = "Press",
            IdealCycleTimeSeconds = 1
        };
        context.ProductionOperations.Add(operation);
        await context.SaveChangesAsync();

        var start = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        context.ProductionEvents.AddRange(
            CreateEvent(order.Id, machine.Id, operation.Id, ProductionEventType.MachineStopped, start.AddSeconds(-10)),
            CreateEvent(order.Id, machine.Id, operation.Id, ProductionEventType.MachineStarted, start.AddSeconds(20)),
            CreateEvent(order.Id, machine.Id, operation.Id, ProductionEventType.ProductionReported, start.AddSeconds(30), 60, 10));
        await context.SaveChangesAsync();

        var result = await new IndustrialOperationsService(context)
            .CalculateOeeAsync(machine.Id, start, start.AddSeconds(100), CancellationToken.None);

        Assert.Equal(20, result.DowntimeSeconds);
        Assert.Equal(80, result.OperatingSeconds);
        Assert.Equal(80, result.AvailabilityPercent);
        Assert.Equal(87.5m, result.PerformancePercent);
        Assert.Equal(85.71m, result.QualityPercent);
        Assert.Equal(60, result.OeePercent);
    }

    private static async Task<OrdemProducao> CreateOrderAsync(AppDbContext context)
    {
        var product = CreateProduct("FIN-003", TipoProduto.PA);
        context.Produtos.Add(product);
        await context.SaveChangesAsync();
        var order = new OrdemProducao { ProductId = product.Id, UsuarioId = 1, Quantity = 100 };
        context.OrdensProducao.Add(order);
        await context.SaveChangesAsync();
        return order;
    }

    private static ProductionEventCreateDto CreateProductionEvent(
        int orderId,
        Guid eventId,
        decimal goodQuantity = 1) => new()
    {
        EventId = eventId,
        OrderId = orderId,
        Type = ProductionEventType.ProductionReported,
        GoodQuantity = goodQuantity,
        OccurredAt = DateTime.UtcNow
    };

    private static ProductionEvent CreateEvent(
        int orderId,
        int machineId,
        int operationId,
        ProductionEventType type,
        DateTime occurredAt,
        decimal goodQuantity = 0,
        decimal scrapQuantity = 0) => new()
    {
        EventId = Guid.NewGuid(),
        OrderId = orderId,
        MachineId = machineId,
        OperationId = operationId,
        Type = type,
        GoodQuantity = goodQuantity,
        ScrapQuantity = scrapQuantity,
        OccurredAt = occurredAt
    };

    private static Produto CreateProduct(string code, TipoProduto type) => new()
    {
        Code = code,
        Name = code,
        Tipo = type,
        Unit = "UN",
        UsuarioId = 1
    };

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"smartprod-industrial-unit-{Guid.NewGuid():N}")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }
}
