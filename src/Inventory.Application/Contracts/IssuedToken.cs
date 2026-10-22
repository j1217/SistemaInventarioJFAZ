namespace Inventory.Application.Contracts;

public sealed record IssuedToken(string AccessToken, DateTimeOffset ExpiresAt);
