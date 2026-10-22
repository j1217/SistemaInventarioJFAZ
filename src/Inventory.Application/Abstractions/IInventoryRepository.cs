using Inventory.Application.Contracts;
using Inventory.Domain.Movements;

namespace Inventory.Application.Abstractions;

public interface IInventoryRepository
{
    Task RegisterAsync(Guid productId, Guid userId, MovementType type, int quantity, string? note, CancellationToken cancellationToken);
    Task<IReadOnlyList<InventoryItemDto>> GetCurrentAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<MovementDto>> GetRecentAsync(int take, CancellationToken cancellationToken);
}
