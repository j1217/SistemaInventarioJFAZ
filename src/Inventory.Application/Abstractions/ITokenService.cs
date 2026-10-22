using Inventory.Domain.Users;
using Inventory.Application.Contracts;

namespace Inventory.Application.Abstractions;

public interface ITokenService
{
    IssuedToken CreateToken(UserAccount user);
}
