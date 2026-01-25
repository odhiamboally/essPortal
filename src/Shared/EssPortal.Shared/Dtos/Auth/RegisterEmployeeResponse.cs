using System;
using System.Collections.Generic;
using System.Text;

namespace EssPortal.Shared.Dtos.Auth;

public record RegisterEmployeeResponse(
    string UserId,
    bool RequiresEmailConfirmation,
    string ConfirmationLink,
    string Token
    );