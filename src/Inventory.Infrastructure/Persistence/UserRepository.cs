using Inventory.Application.Abstractions;
using Inventory.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public sealed class UserRepository(InventoryDbContext database) : IUserRepository
{
    public Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        database.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public async Task AddAsync(UserAccount user, CancellationToken cancellationToken)
    {
        database.Users.Add(user);
        await database.SaveChangesAsync(cancellationToken);
    }
}
