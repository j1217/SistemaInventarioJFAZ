namespace Inventory.Application.Contracts;

public sealed record LoginResultDto(string AccessToken, DateTimeOffset ExpiresAt, string Email, string Role);
