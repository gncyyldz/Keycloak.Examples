using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Modals.Request
{
    public record LoginRequest(string Username = "admin", string Password = "admin");
}
