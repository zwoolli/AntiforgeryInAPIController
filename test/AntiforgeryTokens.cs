using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tests;

public class AntiforgeryTokens
{
    /// <summary>
    /// Gets or sets the name of the cookie to use.
    /// <summary>
    [JsonProperty("cookieName")]
    [JsonPropertyName("cookieName")]
    public string CookieName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value to use for the antiforgery token Http cookie.
    /// </summary>
    [JsonProperty("cookieValue")]
    [JsonPropertyName("cookieValue")]
    public string CookieValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the form parameter to use for the antiforgery token.
    /// </summary>
    [JsonProperty("formFieldName")]
    [JsonPropertyName("formFieldName")]
    public string FormFieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the Http request header to use for the antiforgery token.
    /// </summary>
    [JsonProperty("headerName")]
    [JsonPropertyName("headerName")]
    public string HeaderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value used for the antiforgery token for forms and/or Http request headers.
    /// </summary>
    [JsonProperty("requestToken")]
    [JsonPropertyName("requestToken")]
    public string RequestToken { get; set; } = string.Empty;
}