namespace EssPortal.Web.Mvc.Exceptions;

public class EntityNotFoundException : CustomException
{
    public EntityNotFoundException() : base() { }

    public EntityNotFoundException(string message) : base(message) { }

    public EntityNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
