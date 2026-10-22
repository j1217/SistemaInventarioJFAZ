using Inventory.Application.Abstractions;
using Inventory.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Inventory.Infrastructure.Persistence;

public sealed class ProductRepository(InventoryDbContext database) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken) =>
        await database.Products.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        database.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);

    public Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken) =>
        database.Products.AnyAsync(x => x.Sku == sku, cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        database.Products.Add(product);
        try
        {
            await database.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // La restricción de PostgreSQL protege también escrituras simultáneas.
            throw new Inventory.Application.Errors.ConflictException("Ya existe un producto con ese SKU.");
        }
    }
}
