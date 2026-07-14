using System.Text.Json.Serialization;

namespace CherAmiAPI.Shared.Responses
{
    public class DiscoveryDocument
    {
        [JsonPropertyName("issuer")]
        public string Issuer { get; set; }

        [JsonPropertyName("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; }

        [JsonPropertyName("device_authorization_endpoint")]
        public string DeviceAuthorizationEndpoint { get; set; }

        [JsonPropertyName("token_endpoint")]
        public string TokenEndpoint { get; set; }

        [JsonPropertyName("userinfo_endpoint")]
        public string UserinfoEndpoint { get; set; }

        [JsonPropertyName("revocation_endpoint")]
        public string RevocationEndpoint { get; set; }

        [JsonPropertyName("jwks_uri")]
        public string JwksUri { get; set; }

        [JsonPropertyName("response_types_supported")]
        public string[] ResponseTypesSupported { get; set; }

        [JsonPropertyName("response_modes_supported")]
        public string[] ResponseModesSupported { get; set; }

        [JsonPropertyName("subject_types_supported")]
        public string[] SubjectTypesSupported { get; set; }

        [JsonPropertyName("id_token_signing_alg_values_supported")]
        public string[] IdTokenSigningAlgValuesSupported { get; set; }

        [JsonPropertyName("scopes_supported")]
        public string[] ScopesSupported { get; set; }

        [JsonPropertyName("token_endpoint_auth_methods_supported")]
        public string[] TokenEndpointAuthMethodsSupported { get; set; }

        [JsonPropertyName("claims_supported")]
        public string[] ClaimsSupported { get; set; }

        [JsonPropertyName("code_challenge_methods_supported")]
        public string[] CodeChallengeMethodsSupported { get; set; }

        [JsonPropertyName("grant_types_supported")]
        public string[] GrantTypesSupported { get; set; }
    }
}

