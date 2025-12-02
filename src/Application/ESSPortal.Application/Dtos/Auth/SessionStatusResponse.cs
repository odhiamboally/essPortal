using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSPortal.Application.Dtos.Auth;
public record SessionStatusResponse
{
    public bool IsValid { get; init; }
    public string? UserId { get; init; }
    public string? SessionId { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
}
