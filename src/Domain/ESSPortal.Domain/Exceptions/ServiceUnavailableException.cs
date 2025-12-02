namespace ESSPortal.Domain.Exceptions;
public class ServiceUnavailableException : CustomException
{
    public ServiceUnavailableException(string message = null!) : base(message: message)
    {
    }
}
