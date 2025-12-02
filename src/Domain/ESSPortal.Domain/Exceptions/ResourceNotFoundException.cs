namespace ESSPortal.Domain.Exceptions;
public class ResourceNotFoundException : CustomException
{
    public ResourceNotFoundException(string message = null!) : base(message: message)
    {
    }
}
