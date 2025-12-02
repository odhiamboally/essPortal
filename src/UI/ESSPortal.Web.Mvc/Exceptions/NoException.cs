namespace EssPortal.Web.Mvc.Exceptions;

public class NoException : CustomException
{
    public NoException() : base() { }

    public NoException(string message) : base(message) { }

    public NoException(string message, Exception innerException) : base(message, innerException) { }
}
