using Inventory.Application.Abstractions;
using Inventory.Domain.Users;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Inventory.Infrastructure.Security;

public sealed class AdminSeeder(
    InventoryDbContext database,
    IPasswordHasher passwords,
    IInventoryRepository inventory,
    IConfiguration configuration)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var email = configuration["Seed:AdminEmail"];
        var password = configuration["Seed:AdminPassword"];
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            // Conserva la cuenta existente y crea el administrador solo una vez.
            var normalizedEmail = email.Trim().ToLowerInvariant();
            if (!await database.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken))
            {
                database.Users.Add(new UserAccount(normalizedEmail, passwords.Hash(password)));
                await database.SaveChangesAsync(cancellationToken);
            }
        }

        if (!configuration.GetValue<bool>("Seed:LoadDemoData") || await database.Movements.AnyAsync(cancellationToken))
            return;

        var admin = await database.Users.OrderBy(x => x.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        if (admin is null)
            return;

        var demoStock = new (string Sku, int Quantity)[]
        {
            ("TEC-001", 18), ("TEC-002", 12), ("TEC-003", 7),
            ("ACC-001", 4), ("ACC-002", 9), ("TEC-004", 5)
        };
        foreach (var (sku, quantity) in demoStock)
        {
            var product = await database.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Sku == sku, cancellationToken);
            if (product is not null)
                await inventory.RegisterAsync(product.Id, admin.Id, Inventory.Domain.Movements.MovementType.Entry,
                    quantity, "Saldo inicial de demostración", cancellationToken);
        }
    }
}
