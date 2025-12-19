using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSPortal.Application.Dtos.Auth;


public record UnlockRequest(string Password, string? Email = null, string? EmployeeNumber = null);