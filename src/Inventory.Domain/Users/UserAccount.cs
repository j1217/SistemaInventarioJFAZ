namespace Inventory.Domain.Users;

public sealed class UserAccount
{
    private UserAccount() { }

    public UserAccount(string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = "Administrator";
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = "Administrator";
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
