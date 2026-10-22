using System.Security.Claims;
using Inventory.Api.Contracts;
using Inventory.Application.Contracts;
using Inventory.Application.Services;
using Inventory.Domain.Movements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public sealed class InventoryController(InventoryService inventory) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InventoryItemDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InventoryItemDto>>> Current(CancellationToken cancellationToken) =>
        Ok(await inventory.GetCurrentAsync(cancellationToken));

    [HttpGet("movements")]
    [ProducesResponseType<IReadOnlyList<MovementDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MovementDto>>> Recent([FromQuery] int take = 20, CancellationToken cancellationToken = default) =>
        Ok(await inventory.GetRecentAsync(take, cancellationToken));

    [HttpPost("entries")]
    [ProducesResponseType<MovementResultDto>(StatusCodes.Status201Created)]
    public Task<ActionResult<MovementResultDto>> Entry(RegisterMovementRequest request, CancellationToken cancellationToken) =>
        Register(request, MovementType.Entry, cancellationToken);

    [HttpPost("exits")]
    [ProducesResponseType<MovementResultDto>(StatusCodes.Status201Created)]
    public Task<ActionResult<MovementResultDto>> Exit(RegisterMovementRequest request, CancellationToken cancellationToken) =>
        Register(request, MovementType.Exit, cancellationToken);

    private async Task<ActionResult<MovementResultDto>> Register(RegisterMovementRequest request, MovementType type, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var identity))
            return Unauthorized();

        var result = await inventory.RegisterAsync(request.ProductId, identity, type, request.Quantity, request.Note, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
