using Inventory.Domain.Products;

namespace Inventory.Application.Abstractions;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken);
    Task AddAsync(Product product, CancellationToken cancellationToken);
}
