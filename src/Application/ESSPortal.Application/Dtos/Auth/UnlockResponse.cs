using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSPortal.Application.Dtos.Auth;
public record UnlockResponse
{
    public bool Success { get; init; }
    public bool AccountLocked { get; init; }
    public bool SessionExpired { get; init; }
    public string? SessionId { get; init; }
}
