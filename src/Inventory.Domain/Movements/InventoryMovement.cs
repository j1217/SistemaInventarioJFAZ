using Inventory.Domain.Products;
using Inventory.Domain.Users;

namespace Inventory.Domain.Movements;

public sealed class InventoryMovement
{
    private InventoryMovement() { }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public UserAccount User { get; private set; } = null!;
    public MovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public string? Note { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
