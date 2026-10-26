using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Services;

namespace SmartProd.API.Server.Controllers;

[ApiController]
[Route("api/v1/Industrial")]
[Authorize]
public sealed class IndustrialController(IndustrialOperationsService service, AppDbContext context) : ControllerBase
{
    [HttpPost("work-centers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateWorkCenter(
        [FromBody] WorkCenterCreateDto dto,
        CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await service.CreateWorkCenterAsync(dto, cancellationToken));

    [HttpPost("machines")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateMachine(
        [FromBody] MachineCreateDto dto,
        CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await service.CreateMachineAsync(dto, cancellationToken));

    [HttpPost("operations")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateOperation(
        [FromBody] ProductionOperationCreateDto dto,
        CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await service.CreateOperationAsync(dto, cancellationToken));

    [HttpPost("production-events")]
    public async Task<IActionResult> RecordProductionEvent(
        [FromBody] ProductionEventCreateDto dto,
        CancellationToken cancellationToken)
    {
        int? operatorId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
        var (productionEvent, duplicate) = await service.RecordEventAsync(dto, operatorId, cancellationToken);
        return duplicate ? Ok(productionEvent) : StatusCode(StatusCodes.Status201Created, productionEvent);
    }

    [HttpGet("machines/{machineId:int}/oee")]
    public async Task<IActionResult> GetMachineOee(
        int machineId,
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        CancellationToken cancellationToken) =>
        Ok(await service.CalculateOeeAsync(machineId, start, end, cancellationToken));

    [HttpPost("lots")]
    public async Task<IActionResult> CreateLot(
        [FromBody] LotCreateDto dto,
        CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await service.CreateLotAsync(dto, cancellationToken));

    [HttpPost("lots/consume")]
    public async Task<IActionResult> ConsumeLot(
        [FromBody] LotConsumptionDto dto,
        CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await service.ConsumeLotAsync(dto, cancellationToken));

    [HttpGet("lots/{lotId:int}/traceability")]
    public async Task<IActionResult> GetLotTraceability(int lotId, CancellationToken cancellationToken) =>
        Ok(await service.GetLotTraceabilityAsync(lotId, cancellationToken));

    [HttpGet("audit")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAuditEvents(
        [FromQuery] DateTime? since,
        [FromQuery] DateTime? until,
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 500)
            return BadRequest(new { error = "Page must be positive and pageSize must be between 1 and 500." });

        var query = context.AuditEvents.AsNoTracking();
        if (since.HasValue)
            query = query.Where(audit => audit.OccurredAt >= since.Value);
        if (until.HasValue)
            query = query.Where(audit => audit.OccurredAt < until.Value);
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(audit => audit.EntityType == entityType);
        if (!string.IsNullOrWhiteSpace(entityId))
            query = query.Where(audit => audit.EntityId == entityId);

        var totalCount = await query.CountAsync(cancellationToken);
        var events = await query
            .OrderByDescending(audit => audit.OccurredAt)
            .ThenByDescending(audit => audit.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(new { totalCount, page, pageSize, events });
    }
}
