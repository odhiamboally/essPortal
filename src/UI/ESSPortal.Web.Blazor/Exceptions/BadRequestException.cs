using System.Net;

namespace EssPortal.Web.Blazor.Exceptions;

public class BadRequestException : CustomException
{
    public BadRequestException(string message) : base(message, null, HttpStatusCode.BadRequest) { }
}
