namespace Inventory.Application.Contracts;

public sealed record MovementDto(Guid Id, Guid ProductId, string Sku, string ProductName, string Type, int Quantity, string? Note, string PerformedBy, DateTimeOffset CreatedAt);
