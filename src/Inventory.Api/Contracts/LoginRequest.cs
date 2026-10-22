using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.Contracts;

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password);
