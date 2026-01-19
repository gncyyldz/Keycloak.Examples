using System.Text.Json.Serialization;

namespace Shared.Modals.Request
{
    public class ClientRequest
    {
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("protocol")]
        public string Protocol { get; set; }

        [JsonPropertyName("publicClient")]
        public bool PublicClient { get; set; }

        [JsonPropertyName("serviceAccountsEnabled")]
        public bool ServiceAccountsEnabled { get; set; }

        [JsonPropertyName("directAccessGrantsEnabled")]
        public bool DirectAccessGrantsEnabled { get; set; }
    }
}
