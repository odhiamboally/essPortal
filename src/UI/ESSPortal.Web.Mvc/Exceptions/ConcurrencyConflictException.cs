namespace EssPortal.Web.Mvc.Exceptions;

public class ConcurrencyConflictException : CustomException
{
    public ConcurrencyConflictException() : base() { }

    public ConcurrencyConflictException(string message) : base(message) { }

    public ConcurrencyConflictException(string message, Exception innerException) : base(message, innerException) { }
}
