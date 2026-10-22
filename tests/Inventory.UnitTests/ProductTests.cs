using Inventory.Domain.Products;

namespace Inventory.UnitTests;

public sealed class ProductTests
{
    [Fact]
    public void Constructor_NormalizesSkuAndTrimsText()
    {
        var product = new Product("  abc-01 ", "  Mouse  ", "  Óptico  ", 12.5m, 3);

        Assert.Equal("ABC-01", product.Sku);
        Assert.Equal("Mouse", product.Name);
        Assert.Equal("Óptico", product.Description);
    }

    [Theory]
    [InlineData("", "Nombre", 1, 0)]
    [InlineData("SKU", "", 1, 0)]
    [InlineData("SKU", "Nombre", -1, 0)]
    [InlineData("SKU", "Nombre", 1, -1)]
    public void Constructor_RejectsInvalidValues(string sku, string name, decimal price, int minimumStock)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Product(sku, name, null, price, minimumStock));
    }
}
