namespace EnterpriseUserManagement.Domain.Exceptions;

public class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message) { }
}

public class UserNotFoundException : Exception
{
    public Guid UserId { get; }

    public UserNotFoundException(Guid userId) 
        : base($"User with identifier '{userId}' was not found.")
    {
        UserId = userId;
    }
}

public class DuplicateEmailException : Exception
{
    public string Email { get; }

    public DuplicateEmailException(string email) 
        : base($"A user with the email address '{email}' already exists.")
    {
        Email = email;
    }
}

public class StorageCorruptedException : Exception
{
    public string FilePath { get; }

    public StorageCorruptedException(string filePath, string message, Exception? innerException = null) 
        : base(message, innerException)
    {
        FilePath = filePath;
    }
}

