using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSPortal.Application.Dtos.Auth;
public record UnlockRequest
{
    public string? Password { get; init; }
    public string? Email { get; init; }
    public string? EmployeeNumber { get; init; }
}
