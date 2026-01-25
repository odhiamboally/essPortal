namespace EssPortal.Shared.Exceptions;

public class CreatingDuplicateException : CustomException
{
    public CreatingDuplicateException(string message = null!) : base(message: message)
    {
    }
}
