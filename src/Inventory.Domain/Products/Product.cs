namespace Inventory.Domain.Products;

public sealed class Product
{
    private Product() { }

    public Product(string sku, string name, string? description, decimal unitPrice, int minimumStock)
    {
        Id = Guid.NewGuid();
        Update(sku, name, description, unitPrice, minimumStock);
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int MinimumStock { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public void Update(string sku, string name, string? description, decimal unitPrice, int minimumStock)
    {
        // Centraliza validación y normalización para proteger el modelo de dominio.
        if (string.IsNullOrWhiteSpace(sku) || sku.Length > 40)
            throw new ArgumentException("El SKU es obligatorio y admite hasta 40 caracteres.");
        if (string.IsNullOrWhiteSpace(name) || name.Length > 140)
            throw new ArgumentException("El nombre es obligatorio y admite hasta 140 caracteres.");
        if (unitPrice < 0 || minimumStock < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "El precio y el mínimo no pueden ser negativos.");

        Sku = sku.Trim().ToUpperInvariant();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        UnitPrice = unitPrice;
        MinimumStock = minimumStock;
    }
}
