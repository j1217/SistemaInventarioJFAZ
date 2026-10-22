using Inventory.Application.Abstractions;
using Inventory.Application.Contracts;
using Inventory.Application.Errors;

namespace Inventory.Application.Services;

public sealed class AuthenticationService(IUserRepository users, IPasswordHasher passwords, ITokenService tokens)
{
    public async Task<LoginResultDto> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await users.GetByEmailAsync(email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !user.IsActive || !passwords.Verify(password, user.PasswordHash))
            throw new UnauthorizedException("El correo o la contraseña no son correctos.");

        return new LoginResultDto(tokens.CreateToken(user), DateTimeOffset.UtcNow.AddMinutes(60), user.Email, user.Role);
    }
}
