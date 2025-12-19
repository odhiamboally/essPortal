namespace EssPortal.Web.Blazor.Exceptions;

public class NotFoundException : CustomException
{
    public NotFoundException()
    {
            
    }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }

}
