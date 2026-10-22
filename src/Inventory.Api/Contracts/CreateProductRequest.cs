using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.Contracts;

public sealed record CreateProductRequest(
    [Required, StringLength(40, MinimumLength = 2)] string Sku,
    [Required, StringLength(140, MinimumLength = 2)] string Name,
    [StringLength(500)] string? Description,
    [Range(0, 999999999)] decimal UnitPrice,
    [Range(0, 1000000)] int MinimumStock);
