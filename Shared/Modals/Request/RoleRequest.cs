using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Shared.Modals.Request
{
    public record RoleRequest(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string Description);
}
