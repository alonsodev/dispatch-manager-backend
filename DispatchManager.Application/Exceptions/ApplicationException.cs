namespace DispatchManager.Application.Exceptions;

public class ApplicationException : Exception
{
    public ApplicationException(string message) : base(message) { }

    public ApplicationException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class NotFoundException : ApplicationException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found") { }
}

public class ValidationException : ApplicationException
{
    public ValidationException(string message) : base(message) { }

    public ValidationException(IEnumerable<string> errors)
        : base($"Validation failed: {string.Join(", ", errors)}") { }
}

public class BusinessRuleException : ApplicationException
{
    public BusinessRuleException(string message) : base(message) { }
}
