using System.Text.Json.Serialization;

namespace Shared.Modals.Request
{
    public class RealmRequest
    {
        [JsonPropertyName("realm")]
        public string Realm { get; set; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
    }
}
