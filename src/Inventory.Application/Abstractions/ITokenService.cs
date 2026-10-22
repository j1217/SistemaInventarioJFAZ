using Inventory.Domain.Users;

namespace Inventory.Application.Abstractions;

public interface ITokenService
{
    string CreateToken(UserAccount user);
}
