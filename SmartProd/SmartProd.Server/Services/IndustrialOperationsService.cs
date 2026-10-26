using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Models;
using SmartProd.API.Server.Utils;

namespace SmartProd.API.Server.Services;

public sealed class IndustrialOperationsService(AppDbContext context)
{
    public async Task<WorkCenter> CreateWorkCenterAsync(WorkCenterCreateDto dto, CancellationToken cancellationToken)
    {
        if (await context.WorkCenters.AnyAsync(center => center.Code == dto.Code, cancellationToken))
            throw new ConflictException("A work center with this code already exists.");

        var workCenter = new WorkCenter { Code = dto.Code.Trim(), Name = dto.Name.Trim() };
        context.WorkCenters.Add(workCenter);
        await context.SaveChangesAsync(cancellationToken);
        return workCenter;
    }

    public async Task<Machine> CreateMachineAsync(MachineCreateDto dto, CancellationToken cancellationToken)
    {
        if (!await context.WorkCenters.AnyAsync(center => center.Id == dto.WorkCenterId, cancellationToken))
            throw new NotFoundException("Work center not found.");
        if (await context.Machines.AnyAsync(machine => machine.Code == dto.Code, cancellationToken))
            throw new ConflictException("A machine with this code already exists.");

        var machine = new Machine
        {
            WorkCenterId = dto.WorkCenterId,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim()
        };
        context.Machines.Add(machine);
        await context.SaveChangesAsync(cancellationToken);
        return machine;
    }

    public async Task<ProductionOperation> CreateOperationAsync(
        ProductionOperationCreateDto dto,
        CancellationToken cancellationToken)
    {
        if (!await context.Produtos.AnyAsync(product => product.Id == dto.ProductId, cancellationToken))
            throw new NotFoundException("Product not found.");
        if (!await context.WorkCenters.AnyAsync(center => center.Id == dto.WorkCenterId, cancellationToken))
            throw new NotFoundException("Work center not found.");
        if (await context.ProductionOperations.AnyAsync(
                operation => operation.ProductId == dto.ProductId && operation.Sequence == dto.Sequence,
                cancellationToken))
            throw new ConflictException("An operation with this sequence already exists for the product.");

        var operation = new ProductionOperation
        {
            ProductId = dto.ProductId,
            WorkCenterId = dto.WorkCenterId,
            Sequence = dto.Sequence,
            Name = dto.Name.Trim(),
            IdealCycleTimeSeconds = dto.IdealCycleTimeSeconds
        };
        context.ProductionOperations.Add(operation);
        await context.SaveChangesAsync(cancellationToken);
        return operation;
    }

    public async Task<LotTraceabilityDto> GetLotTraceabilityAsync(int lotId, CancellationToken cancellationToken)
    {
        var lot = await context.Lots
            .AsNoTracking()
            .Where(candidate => candidate.Id == lotId)
            .Select(candidate => new LotTraceabilityDto(
                candidate.Id,
                candidate.Number,
                candidate.Product.Name,
                candidate.SupplierLotNumber,
                candidate.QuantityReceived,
                candidate.QuantityRemaining,
                candidate.ExpiresAt,
                candidate.ProductionOrderId,
                Array.Empty<LotGenealogyEdgeDto>(),
                Array.Empty<LotGenealogyEdgeDto>()))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Lot not found.");

        var relationships = await context.LotGenealogy
            .AsNoTracking()
            .Select(record => new
            {
                record.InputLotId,
                InputLotNumber = record.InputLot.Number,
                InputProductName = record.InputLot.Product.Name,
                record.OutputLotId,
                OutputLotNumber = record.OutputLot.Number,
                OutputProductName = record.OutputLot.Product.Name,
                record.ProductionOrderId,
                record.QuantityConsumed,
                record.RecordedAt
            })
            .ToListAsync(cancellationToken);

        var ancestors = new HashSet<int>();
        var descendants = new HashSet<int>();
        var pendingAncestors = new Queue<int>();
        var pendingDescendants = new Queue<int>();
        pendingAncestors.Enqueue(lotId);
        pendingDescendants.Enqueue(lotId);

        while (pendingAncestors.TryDequeue(out var current))
        {
            foreach (var relationship in relationships.Where(item => item.OutputLotId == current))
            {
                if (ancestors.Add(relationship.InputLotId))
                    pendingAncestors.Enqueue(relationship.InputLotId);
            }
        }

        while (pendingDescendants.TryDequeue(out var current))
        {
            foreach (var relationship in relationships.Where(item => item.InputLotId == current))
            {
                if (descendants.Add(relationship.OutputLotId))
                    pendingDescendants.Enqueue(relationship.OutputLotId);
            }
        }

        return lot with
        {
            Inputs = relationships
                .Where(item => ancestors.Contains(item.InputLotId) && ancestors.Contains(item.OutputLotId) ||
                               item.OutputLotId == lotId)
                .Select(item => new LotGenealogyEdgeDto(
                    item.InputLotId,
                    item.InputLotNumber,
                    item.InputProductName,
                    item.OutputLotId,
                    item.OutputLotNumber,
                    item.OutputProductName,
                    item.ProductionOrderId,
                    item.QuantityConsumed,
                    item.RecordedAt))
                .ToList(),
            Outputs = relationships
                .Where(item => descendants.Contains(item.InputLotId) && descendants.Contains(item.OutputLotId) ||
                               item.InputLotId == lotId)
                .Select(item => new LotGenealogyEdgeDto(
                    item.InputLotId,
                    item.InputLotNumber,
                    item.InputProductName,
                    item.OutputLotId,
                    item.OutputLotNumber,
                    item.OutputProductName,
                    item.ProductionOrderId,
                    item.QuantityConsumed,
                    item.RecordedAt))
                .ToList()
        };
    }

    public async Task<(ProductionEvent Event, bool Duplicate)> RecordEventAsync(
        ProductionEventCreateDto dto,
        int? operatorId,
        CancellationToken cancellationToken)
    {
        if (dto.EventId == Guid.Empty)
            throw new ValidationException("EventId must be a non-empty UUID.");
        if (!System.Enum.IsDefined(dto.Type))
            throw new ValidationException("Production event type is invalid.");
        if (dto.GoodQuantity < 0 || dto.ScrapQuantity < 0 || dto.ReworkQuantity < 0)
            throw new ValidationException("Production quantities cannot be negative.");
        if ((dto.DeviceId is null) != (dto.DeviceSequence is null))
            throw new ValidationException("DeviceId and DeviceSequence must be supplied together.");
        if (dto.DeviceId is not null && string.IsNullOrWhiteSpace(dto.DeviceId))
            throw new ValidationException("DeviceId cannot be empty.");

        var existing = await context.ProductionEvents
            .FirstOrDefaultAsync(productionEvent => productionEvent.EventId == dto.EventId, cancellationToken);
        if (existing is not null)
        {
            if (existing.OrderId != dto.OrderId ||
                existing.OperationId != dto.OperationId ||
                existing.MachineId != dto.MachineId ||
                existing.Type != dto.Type ||
                existing.GoodQuantity != dto.GoodQuantity ||
                existing.ScrapQuantity != dto.ScrapQuantity ||
                existing.ReworkQuantity != dto.ReworkQuantity ||
                existing.ReasonCode != dto.ReasonCode?.Trim() ||
                existing.DeviceId != dto.DeviceId ||
                existing.DeviceSequence != dto.DeviceSequence ||
                existing.OccurredAt != DateTime.SpecifyKind(dto.OccurredAt, DateTimeKind.Utc))
                throw new ConflictException("EventId was already recorded with different event data.");

            return (existing, true);
        }
        if (dto.DeviceId is not null && await context.ProductionEvents.AnyAsync(
                productionEvent => productionEvent.DeviceId == dto.DeviceId &&
                                   productionEvent.DeviceSequence == dto.DeviceSequence,
                cancellationToken))
            throw new ConflictException("The device sequence was already recorded.");

        var order = await context.OrdensProducao
            .FirstOrDefaultAsync(candidate => candidate.Id == dto.OrderId, cancellationToken)
            ?? throw new NotFoundException("Production order not found.");

        ProductionOperation? operation = null;
        if (dto.OperationId.HasValue)
        {
            operation = await context.ProductionOperations
                .FirstOrDefaultAsync(candidate => candidate.Id == dto.OperationId, cancellationToken)
                ?? throw new NotFoundException("Production operation not found.");
            if (operation.ProductId != order.ProductId)
                throw new ValidationException("The operation does not belong to the order's product.");
        }

        Machine? machine = null;
        if (dto.MachineId.HasValue)
        {
            machine = await context.Machines
                .FirstOrDefaultAsync(candidate => candidate.Id == dto.MachineId, cancellationToken)
                ?? throw new NotFoundException("Machine not found.");
        }

        if (dto.Type == ProductionEventType.MachineStopped && string.IsNullOrWhiteSpace(dto.ReasonCode))
            throw new ValidationException("A reason code is required when a machine stops.");
        if (dto.Type is ProductionEventType.ScrapReported or ProductionEventType.ReworkReported &&
            string.IsNullOrWhiteSpace(dto.ReasonCode))
            throw new ValidationException("A reason code is required for scrap and rework events.");

        var productionEvent = new ProductionEvent
        {
            EventId = dto.EventId,
            OrderId = dto.OrderId,
            OperationId = operation?.Id,
            MachineId = machine?.Id,
            OperatorId = operatorId,
            Type = dto.Type,
            GoodQuantity = dto.GoodQuantity,
            ScrapQuantity = dto.ScrapQuantity,
            ReworkQuantity = dto.ReworkQuantity,
            ReasonCode = dto.ReasonCode?.Trim(),
            DeviceId = dto.DeviceId,
            DeviceSequence = dto.DeviceSequence,
            OccurredAt = DateTime.SpecifyKind(dto.OccurredAt, DateTimeKind.Utc),
            ReceivedAt = DateTime.UtcNow
        };

        if (dto.Type is ProductionEventType.ProductionReported or ProductionEventType.ScrapReported or ProductionEventType.ReworkReported)
        {
            var quantity = dto.GoodQuantity + dto.ScrapQuantity + dto.ReworkQuantity;
            if (quantity <= 0)
                throw new ValidationException("A production event must report a positive quantity.");
            order.Produced += (double)dto.GoodQuantity;
        }

        if (machine is not null)
        {
            machine.Status = dto.Type switch
            {
                ProductionEventType.MachineStopped => "Stopped",
                ProductionEventType.MachineStarted => "Running",
                _ => machine.Status
            };
        }

        context.ProductionEvents.Add(productionEvent);
        await context.SaveChangesAsync(cancellationToken);
        return (productionEvent, false);
    }

    public async Task<Lot> CreateLotAsync(LotCreateDto dto, CancellationToken cancellationToken)
    {
        if (dto.Quantity <= 0)
            throw new ValidationException("Lot quantity must be greater than zero.");
        if (!System.Enum.IsDefined(dto.Kind))
            throw new ValidationException("Lot kind is invalid.");
        if (string.IsNullOrWhiteSpace(dto.Number))
            throw new ValidationException("Lot number is required.");

        var product = await context.Produtos
            .FirstOrDefaultAsync(candidate => candidate.Id == dto.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        if (dto.Kind == LotKind.RawMaterial && product.Tipo != TipoProduto.MP)
            throw new ValidationException("Raw-material lots can only be created for MP products.");

        OrdemProducao? order = null;
        if (dto.Kind == LotKind.FinishedGood)
        {
            if (!dto.ProductionOrderId.HasValue)
                throw new ValidationException("A production order is required for a finished-goods lot.");
            if (product.Tipo != TipoProduto.PA)
                throw new ValidationException("Finished-goods lots can only be created for PA products.");

            order = await context.OrdensProducao
                .FirstOrDefaultAsync(candidate => candidate.Id == dto.ProductionOrderId, cancellationToken)
                ?? throw new NotFoundException("Production order not found.");
            if (order.ProductId != product.Id)
                throw new ValidationException("The production order does not produce the selected product.");
        }
        else if (dto.ProductionOrderId.HasValue)
        {
            throw new ValidationException("Raw-material receipt lots cannot reference a production order.");
        }

        var duplicate = await context.Lots.AnyAsync(
            lot => lot.ProductId == dto.ProductId && lot.Number == dto.Number,
            cancellationToken);
        if (duplicate)
            throw new ValidationException("A lot with this number already exists for the product.");

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var lot = new Lot
        {
            ProductId = product.Id,
            ProductionOrderId = order?.Id,
            Number = dto.Number.Trim(),
            SupplierLotNumber = dto.SupplierLotNumber?.Trim(),
            QuantityReceived = dto.Quantity,
            QuantityRemaining = dto.Quantity,
            UnitCost = dto.UnitCost,
            ExpiresAt = dto.ExpiresAt
        };
        context.Lots.Add(lot);
        product.EstoqueAtual += (double)dto.Quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await UpdateStockSnapshotAsync(product, cancellationToken);
        context.Movimentacoes.Add(new Movimentacao
        {
            ProductId = product.Id,
            Tipo = dto.Kind == LotKind.RawMaterial ? TipoMovimentacao.ENTRADA : TipoMovimentacao.PRODUCAO,
            Quantity = (double)dto.Quantity,
            OrderId = order?.Id,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return lot;
    }

    public async Task<LotGenealogy> ConsumeLotAsync(LotConsumptionDto dto, CancellationToken cancellationToken)
    {
        if (dto.Quantity <= 0)
            throw new ValidationException("Lot consumption quantity must be greater than zero.");

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var inputLot = await context.Lots
            .Include(lot => lot.Product)
            .FirstOrDefaultAsync(lot => lot.Id == dto.InputLotId, cancellationToken)
            ?? throw new NotFoundException("Input lot not found.");
        var outputLot = await context.Lots
            .Include(lot => lot.Product)
            .FirstOrDefaultAsync(lot => lot.Id == dto.OutputLotId, cancellationToken)
            ?? throw new NotFoundException("Output lot not found.");
        var order = await context.OrdensProducao
            .FirstOrDefaultAsync(candidate => candidate.Id == dto.ProductionOrderId, cancellationToken)
            ?? throw new NotFoundException("Production order not found.");

        if (inputLot.Id == outputLot.Id)
            throw new ValidationException("A lot cannot be consumed into itself.");
        if (inputLot.Product.Tipo != TipoProduto.MP || outputLot.Product.Tipo != TipoProduto.PA)
            throw new ValidationException("Genealogy requires a raw-material input lot and a finished-goods output lot.");
        if (outputLot.ProductionOrderId != order.Id || order.ProductId != outputLot.ProductId)
            throw new ValidationException("The output lot and production order do not match.");
        if (inputLot.QuantityRemaining < dto.Quantity)
            throw new ValidationException("The input lot does not have enough remaining quantity.");
        if (inputLot.Product.EstoqueAtual < (double)dto.Quantity)
            throw new ValidationException("The input product does not have enough inventory.");
        if (await context.LotGenealogy.AnyAsync(
                genealogy => genealogy.InputLotId == dto.InputLotId &&
                             genealogy.OutputLotId == dto.OutputLotId &&
                             genealogy.ProductionOrderId == dto.ProductionOrderId,
                cancellationToken))
            throw new ValidationException("This lot genealogy relationship already exists; use a new consumption event.");

        inputLot.QuantityRemaining -= dto.Quantity;
        inputLot.Product.EstoqueAtual -= (double)dto.Quantity;
        inputLot.Product.UpdatedAt = DateTime.UtcNow;
        await UpdateStockSnapshotAsync(inputLot.Product, cancellationToken);

        if (inputLot.UnitCost.HasValue && outputLot.QuantityReceived > 0)
        {
            var priorCost = outputLot.UnitCost.GetValueOrDefault() * (outputLot.QuantityReceived - dto.Quantity);
            outputLot.UnitCost = Math.Max(0, (priorCost + inputLot.UnitCost.Value * dto.Quantity) / outputLot.QuantityReceived);
        }

        var genealogyRecord = new LotGenealogy
        {
            InputLotId = inputLot.Id,
            OutputLotId = outputLot.Id,
            ProductionOrderId = order.Id,
            QuantityConsumed = dto.Quantity
        };
        context.LotGenealogy.Add(genealogyRecord);
        context.Movimentacoes.Add(new Movimentacao
        {
            ProductId = inputLot.ProductId,
            Tipo = TipoMovimentacao.CONSUMO,
            Quantity = (double)dto.Quantity,
            OrderId = order.Id,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return genealogyRecord;
    }

    public async Task<OeeResponseDto> CalculateOeeAsync(
        int machineId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken)
    {
        if (end <= start)
            throw new ValidationException("The reporting period end must be later than its start.");

        var machine = await context.Machines
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == machineId, cancellationToken)
            ?? throw new NotFoundException("Machine not found.");

        var previousMachineState = await context.ProductionEvents
            .AsNoTracking()
            .Where(productionEvent =>
                productionEvent.MachineId == machineId &&
                productionEvent.OccurredAt < start &&
                (productionEvent.Type == ProductionEventType.MachineStarted ||
                 productionEvent.Type == ProductionEventType.MachineStopped))
            .OrderByDescending(productionEvent => productionEvent.OccurredAt)
            .Select(productionEvent => (ProductionEventType?)productionEvent.Type)
            .FirstOrDefaultAsync(cancellationToken);

        var events = await context.ProductionEvents
            .AsNoTracking()
            .Where(productionEvent =>
                productionEvent.MachineId == machineId &&
                productionEvent.OccurredAt >= start &&
                productionEvent.OccurredAt < end)
            .OrderBy(productionEvent => productionEvent.OccurredAt)
            .Select(productionEvent => new
            {
                productionEvent.Type,
                productionEvent.OccurredAt,
                productionEvent.GoodQuantity,
                productionEvent.ScrapQuantity,
                productionEvent.ReworkQuantity,
                IdealCycleTime = productionEvent.Operation == null
                    ? 0m
                    : productionEvent.Operation.IdealCycleTimeSeconds
            })
            .ToListAsync(cancellationToken);

        decimal downtimeSeconds = 0;
        DateTime? stoppedAt = previousMachineState == ProductionEventType.MachineStopped ? start : null;
        foreach (var productionEvent in events)
        {
            if (productionEvent.Type == ProductionEventType.MachineStopped)
            {
                stoppedAt ??= productionEvent.OccurredAt < start ? start : productionEvent.OccurredAt;
            }
            else if (productionEvent.Type == ProductionEventType.MachineStarted && stoppedAt.HasValue)
            {
                var stoppedUntil = productionEvent.OccurredAt > end ? end : productionEvent.OccurredAt;
                if (stoppedUntil > stoppedAt.Value)
                    downtimeSeconds += (decimal)(stoppedUntil - stoppedAt.Value).TotalSeconds;
                stoppedAt = null;
            }
        }

        if (stoppedAt.HasValue && end > stoppedAt.Value)
            downtimeSeconds += (decimal)(end - stoppedAt.Value).TotalSeconds;

        var inPeriod = events.Where(productionEvent => productionEvent.OccurredAt >= start && productionEvent.OccurredAt < end);
        var goodQuantity = inPeriod.Sum(productionEvent => productionEvent.GoodQuantity);
        var totalQuantity = inPeriod.Sum(productionEvent =>
            productionEvent.GoodQuantity + productionEvent.ScrapQuantity + productionEvent.ReworkQuantity);
        var idealRunSeconds = inPeriod.Sum(productionEvent =>
            (productionEvent.GoodQuantity + productionEvent.ScrapQuantity + productionEvent.ReworkQuantity) *
            productionEvent.IdealCycleTime);
        var scheduledSeconds = (decimal)(end - start).TotalSeconds;
        var operatingSeconds = Math.Max(0, scheduledSeconds - downtimeSeconds);
        var availability = scheduledSeconds > 0 ? downtimeSeconds < scheduledSeconds
            ? operatingSeconds / scheduledSeconds * 100
            : 0
            : 0;
        var performance = operatingSeconds > 0 ? idealRunSeconds / operatingSeconds * 100 : 0;
        var quality = totalQuantity > 0 ? goodQuantity / totalQuantity * 100 : 0;

        return new OeeResponseDto(
            machine.Id,
            start,
            end,
            scheduledSeconds,
            downtimeSeconds,
            operatingSeconds,
            goodQuantity,
            totalQuantity,
            decimal.Round(availability, 2),
            decimal.Round(performance, 2),
            decimal.Round(quality, 2),
            decimal.Round(availability * performance * quality / 10000, 2));
    }

    private async Task UpdateStockSnapshotAsync(Produto product, CancellationToken cancellationToken)
    {
        var snapshot = await context.Estoque
            .FirstOrDefaultAsync(stock => stock.ProductId == product.Id, cancellationToken);
        if (snapshot is null)
        {
            if (product.EstoqueAtual < 0)
                throw new ValidationException("Product inventory cannot be negative.");
            context.Estoque.Add(new Estoque
            {
                ProductId = product.Id,
                Quantity = product.EstoqueAtual,
                Unit = product.Unit
            });
            return;
        }

        snapshot.Quantity = product.EstoqueAtual;
        if (snapshot.Quantity < 0)
            throw new ValidationException("The stock snapshot cannot become negative.");
    }
}
