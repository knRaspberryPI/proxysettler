using System.Text.Json.Serialization;

namespace ProxySettler;

internal sealed class IpInfo
{
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("cc")]
    public string? CountryCode { get; set; }
}
