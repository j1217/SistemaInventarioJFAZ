using Inventory.Application.Abstractions;
using Inventory.Application.Contracts;
using Inventory.Application.Errors;
using Inventory.Domain.Movements;

namespace Inventory.Application.Services;

public sealed class InventoryService(IInventoryRepository inventory, IProductRepository products)
{
    public Task<IReadOnlyList<InventoryItemDto>> GetCurrentAsync(CancellationToken cancellationToken) => inventory.GetCurrentAsync(cancellationToken);

    // Limita la consulta para mantener acotado el tamaño de la respuesta.
    public Task<IReadOnlyList<MovementDto>> GetRecentAsync(int take, CancellationToken cancellationToken) =>
        inventory.GetRecentAsync(Math.Clamp(take, 1, 100), cancellationToken);

    public async Task<MovementResultDto> RegisterAsync(Guid productId, Guid userId, MovementType type, int quantity, string? note, CancellationToken cancellationToken)
    {
        if (quantity <= 0)
            throw new BusinessRuleException("La cantidad debe ser mayor que cero.");
        if (note?.Length > 240)
            throw new BusinessRuleException("La nota admite hasta 240 caracteres.");
        if (await products.GetByIdAsync(productId, cancellationToken) is null)
            throw new NotFoundException("El producto solicitado no existe.");

        await inventory.RegisterAsync(productId, userId, type, quantity, note, cancellationToken);
        // Lee el saldo confirmado por PostgreSQL después del movimiento.
        var current = await inventory.GetCurrentAsync(cancellationToken);
        var item = current.Single(x => x.ProductId == productId);
        return new MovementResultDto(productId, type.ToString(), quantity, item.CurrentStock, DateTimeOffset.UtcNow);
    }
}
