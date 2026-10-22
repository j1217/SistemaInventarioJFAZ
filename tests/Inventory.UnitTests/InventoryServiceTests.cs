using Inventory.Application.Abstractions;
using Inventory.Application.Contracts;
using Inventory.Application.Errors;
using Inventory.Application.Services;
using Inventory.Domain.Movements;
using Inventory.Domain.Products;

namespace Inventory.UnitTests;

public sealed class InventoryServiceTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(101, 100)]
    public async Task GetRecentAsync_ClampsRequestedSize(int requested, int expected)
    {
        var repository = new FakeInventoryRepository();
        var service = new InventoryService(repository, new FakeProductRepository());

        await service.GetRecentAsync(requested, CancellationToken.None);

        Assert.Equal(expected, repository.LastTake);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RegisterAsync_RejectsNonPositiveQuantity(int quantity)
    {
        var service = new InventoryService(new FakeInventoryRepository(), new FakeProductRepository());

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.RegisterAsync(Guid.NewGuid(), Guid.NewGuid(), MovementType.Entry, quantity, null, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_RejectsNoteLongerThan240Characters()
    {
        var service = new InventoryService(new FakeInventoryRepository(), new FakeProductRepository());

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.RegisterAsync(Guid.NewGuid(), Guid.NewGuid(), MovementType.Entry, 1, new string('a', 241), CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_RejectsUnknownProduct()
    {
        var service = new InventoryService(new FakeInventoryRepository(), new FakeProductRepository());

        await Assert.ThrowsAsync<NotFoundException>(() => service.RegisterAsync(Guid.NewGuid(), Guid.NewGuid(), MovementType.Entry, 1, null, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_RecordsMovementAndReturnsUpdatedStock()
    {
        var productId = Guid.NewGuid();
        var inventory = new FakeInventoryRepository { StockProductId = productId, CurrentStock = 14 };
        var products = new FakeProductRepository { Product = new Product("SKU-1", "Teclado", null, 10m, 2) };
        var service = new InventoryService(inventory, products);

        var result = await service.RegisterAsync(productId, Guid.NewGuid(), MovementType.Exit, 2, "Venta", CancellationToken.None);

        Assert.True(inventory.WasRegistered);
        Assert.Equal("Exit", result.Type);
        Assert.Equal(2, result.Quantity);
        Assert.Equal(14, result.CurrentStock);
    }

    private sealed class FakeInventoryRepository : IInventoryRepository
    {
        public int LastTake { get; private set; }
        public bool WasRegistered { get; private set; }
        public Guid StockProductId { get; set; }
        public int CurrentStock { get; set; }

        public Task RegisterAsync(Guid productId, Guid userId, MovementType type, int quantity, string? note, CancellationToken cancellationToken)
        {
            WasRegistered = true;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<InventoryItemDto>> GetCurrentAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<InventoryItemDto>>([new(StockProductId, "SKU-1", "Teclado", 10m, 2, CurrentStock, "Disponible")]);

        public Task<IReadOnlyList<MovementDto>> GetRecentAsync(int take, CancellationToken cancellationToken)
        {
            LastTake = take;
            return Task.FromResult<IReadOnlyList<MovementDto>>([]);
        }
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public Product? Product { get; set; }
        public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Product>>([]);
        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Product);
        public Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task AddAsync(Product product, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
