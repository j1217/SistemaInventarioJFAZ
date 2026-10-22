namespace Inventory.Application.Errors;

public abstract class ApplicationExceptionBase(string message) : Exception(message) { }

public sealed class NotFoundException(string message) : ApplicationExceptionBase(message);
public sealed class ConflictException(string message) : ApplicationExceptionBase(message);
public sealed class UnauthorizedException(string message) : ApplicationExceptionBase(message);
public sealed class BusinessRuleException(string message) : ApplicationExceptionBase(message);
