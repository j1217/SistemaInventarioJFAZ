using Inventory.Application.Abstractions;
using Inventory.Application.Contracts;
using Inventory.Application.Errors;

namespace Inventory.Application.Services;

public sealed class AuthenticationService(IUserRepository users, IPasswordHasher passwords, ITokenService tokens)
{
    public async Task<LoginResultDto> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await users.GetByEmailAsync(email.Trim().ToLowerInvariant(), cancellationToken);
        // El mensaje genérico evita revelar si una cuenta existe.
        if (user is null || !user.IsActive || !passwords.Verify(password, user.PasswordHash))
            throw new UnauthorizedException("El correo o la contraseña no son correctos.");

        var issuedToken = tokens.CreateToken(user);
        return new LoginResultDto(issuedToken.AccessToken, issuedToken.ExpiresAt, user.Email, user.Role);
    }
}
