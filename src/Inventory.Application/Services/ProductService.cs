using Inventory.Application.Abstractions;
using Inventory.Application.Contracts;
using Inventory.Application.Errors;
using Inventory.Domain.Products;

namespace Inventory.Application.Services;

public sealed class ProductService(IProductRepository products)
{
    public async Task<IReadOnlyList<ProductDto>> ListAsync(CancellationToken cancellationToken) =>
        (await products.ListAsync(cancellationToken)).Select(Map).ToArray();

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await products.GetByIdAsync(id, cancellationToken) is { } product ? Map(product) : null;

    public async Task<ProductDto> CreateAsync(string sku, string name, string? description, decimal unitPrice, int minimumStock, CancellationToken cancellationToken)
    {
        // Normaliza el SKU antes de revisar la unicidad.
        var normalizedSku = sku.Trim().ToUpperInvariant();
        if (await products.SkuExistsAsync(normalizedSku, cancellationToken))
            throw new ConflictException("Ya existe un producto con ese SKU.");

        var product = new Product(normalizedSku, name, description, unitPrice, minimumStock);
        await products.AddAsync(product, cancellationToken);
        return Map(product);
    }

    private static ProductDto Map(Product product) => new(product.Id, product.Sku, product.Name, product.Description, product.UnitPrice, product.MinimumStock, product.IsActive);
}
