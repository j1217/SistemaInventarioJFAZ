using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.Contracts;

public sealed record RegisterMovementRequest(
    [Required] Guid ProductId,
    [Range(1, 1000000)] int Quantity,
    [StringLength(240)] string? Note);
