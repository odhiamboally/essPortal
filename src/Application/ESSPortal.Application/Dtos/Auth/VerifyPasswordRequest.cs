using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSPortal.Application.Dtos.Auth;
public record VerifyPasswordRequest(
    string UserId,
    string Email,
    string EmployeeNumber,
    string Password
);

