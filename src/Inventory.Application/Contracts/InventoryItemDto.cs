namespace Inventory.Application.Contracts;

public sealed record InventoryItemDto(Guid ProductId, string Sku, string Name, decimal UnitPrice, int MinimumStock, int CurrentStock, string Status);
