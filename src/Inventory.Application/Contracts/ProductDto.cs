namespace Inventory.Application.Contracts;

public sealed record ProductDto(Guid Id, string Sku, string Name, string? Description, decimal UnitPrice, int MinimumStock, bool IsActive);
