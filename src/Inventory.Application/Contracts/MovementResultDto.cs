namespace Inventory.Application.Contracts;

public sealed record MovementResultDto(Guid ProductId, string Type, int Quantity, int CurrentStock, DateTimeOffset CreatedAt);
