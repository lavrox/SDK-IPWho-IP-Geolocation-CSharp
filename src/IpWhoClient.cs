// IPWho C# SDK v1.0.0
//
// Enterprise-grade client for the IPWho IP Geolocation API.
// API docs: https://api.ipwho.org
//
// Dependencies (.csproj):
//   <PackageReference Include="System.Net.Http.Json" Version="8.0.0" />
//
// Usage:
//   var client = new IpWhoClient("sk.xxxx");
//   var resp = await client.LookupAsync("8.8.8.8");
//   Console.WriteLine(resp.Data.GeoLocation?.City);

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace IPWho
{
    // ═══════════════════════════════════════════════════════════════════════
    // Version
    // ═══════════════════════════════════════════════════════════════════════

    internal static class VersionInfo
    {
        public const string Version = "1.0.0";
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Exceptions
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Base exception for IPWho API errors.
    /// </summary>
    public class IpWhoException : Exception
    {
        public int StatusCode { get; }

        public IpWhoException(int statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public IpWhoException(string message, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = 0;
        }
    }

    /// <summary>
    /// Thrown when the API returns 404 (IP not found).
    /// </summary>
    public class NotFoundException : IpWhoException
    {
        public NotFoundException(int statusCode, string message)
            : base(statusCode, message) { }
    }

    /// <summary>
    /// Thrown when the API returns 429 (rate limit exceeded).
    /// </summary>
    public class RateLimitException : IpWhoException
    {
        public RateLimitException(int statusCode, string message)
            : base(statusCode, message) { }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Domain models — mirrors OpenAPI components/schemas exactly.
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Top-level API response wrapper.
    /// </summary>
    public class IpGeoResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public GeoData? Data { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    /// <summary>
    /// The data payload inside a successful IpGeoResponse.
    /// </summary>
    public class GeoData
    {
        [JsonPropertyName("ip")]
        public string Ip { get; set; } = "";

        [JsonPropertyName("geoLocation")]
        public GeoLocation? GeoLocation { get; set; }

        [JsonPropertyName("timezone")]
        public Timezone? Timezone { get; set; }

        [JsonPropertyName("flag")]
        public Flag? Flag { get; set; }

        [JsonPropertyName("currency")]
        public Currency? Currency { get; set; }

        [JsonPropertyName("connection")]
        public Connection? Connection { get; set; }

        [JsonPropertyName("security")]
        public Security? Security { get; set; }

        [JsonPropertyName("userAgent")]
        public UserAgent? UserAgent { get; set; }
    }

    /// <summary>
    /// GeoLocation object.
    /// </summary>
    public class GeoLocation
    {
        [JsonPropertyName("continent")]
        public string? Continent { get; set; }

        [JsonPropertyName("continentCode")]
        public string? ContinentCode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("capital")]
        public string? Capital { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("regionCode")]
        public string? RegionCode { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("postal_Code")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("dial_code")]
        public string? DialCode { get; set; }

        [JsonPropertyName("is_in_eu")]
        public bool? IsInEu { get; set; }

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("accuracy_radius")]
        public double? AccuracyRadius { get; set; }
    }

    /// <summary>
    /// Timezone object.
    /// </summary>
    public class Timezone
    {
        [JsonPropertyName("time_zone")]
        public string? TimeZone { get; set; }

        [JsonPropertyName("abbr")]
        public string? Abbr { get; set; }

        [JsonPropertyName("offset")]
        public double? Offset { get; set; }

        [JsonPropertyName("is_dst")]
        public bool? IsDst { get; set; }

        [JsonPropertyName("utc")]
        public string? Utc { get; set; }

        [JsonPropertyName("current_time")]
        public string? CurrentTime { get; set; }
    }

    /// <summary>
    /// Flag object.
    /// </summary>
    public class Flag
    {
        [JsonPropertyName("flag_Icon")]
        public string? FlagIcon { get; set; }

        [JsonPropertyName("flag_unicode")]
        public string? FlagUnicode { get; set; }
    }

    /// <summary>
    /// Currency object.
    /// </summary>
    public class Currency
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("name_plural")]
        public string? NamePlural { get; set; }

        [JsonPropertyName("hex_unicode")]
        public string? HexUnicode { get; set; }
    }

    /// <summary>
    /// Connection object.
    /// </summary>
    public class Connection
    {
        [JsonPropertyName("asn_number")]
        public double? AsnNumber { get; set; }

        [JsonPropertyName("asn_org")]
        public string? AsnOrg { get; set; }

        [JsonPropertyName("isp")]
        public string? Isp { get; set; }

        [JsonPropertyName("org")]
        public string? Org { get; set; }

        [JsonPropertyName("domain")]
        public string? Domain { get; set; }

        [JsonPropertyName("connection_type")]
        public string? ConnectionType { get; set; }
    }

    /// <summary>
    /// Security object.
    /// </summary>
    public class Security
    {
        [JsonPropertyName("isVpn")]
        public bool? IsVpn { get; set; }

        [JsonPropertyName("isTor")]
        public bool? IsTor { get; set; }

        [JsonPropertyName("isThreat")]
        public string? IsThreat { get; set; }  // "low" | "medium" | "high"
    }

    /// <summary>
    /// UserAgent object.
    /// </summary>
    public class UserAgent
    {
        [JsonPropertyName("browser")]
        public BrowserInfo? Browser { get; set; }

        [JsonPropertyName("engine")]
        public EngineInfo? Engine { get; set; }

        [JsonPropertyName("os")]
        public OSInfo? Os { get; set; }

        [JsonPropertyName("device")]
        public DeviceInfo? Device { get; set; }

        [JsonPropertyName("cpu")]
        public CPUInfo? Cpu { get; set; }
    }

    /// <summary>
    /// Browser sub-object within UserAgent.
    /// </summary>
    public class BrowserInfo
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }
    }

    /// <summary>
    /// Engine sub-object within UserAgent.
    /// </summary>
    public class EngineInfo
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }
    }

    /// <summary>
    /// OS sub-object within UserAgent.
    /// </summary>
    public class OSInfo
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }
    }

    /// <summary>
    /// Device sub-object within UserAgent.
    /// </summary>
    public class DeviceInfo
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("vendor")]
        public string? Vendor { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }
    }

    /// <summary>
    /// CPU sub-object within UserAgent.
    /// </summary>
    public class CPUInfo
    {
        [JsonPropertyName("architecture")]
        public string? Architecture { get; set; }
    }

    /// <summary>
    /// Wrapper for the /bulk/ endpoint response.
    /// </summary>
    public class BulkResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public BulkResponseData? Data { get; set; }
    }

    /// <summary>
    /// Nested data wrapper inside BulkResponse.
    /// </summary>
    public class BulkResponseData
    {
        [JsonPropertyName("responseArray")]
        public List<IpGeoResponse>? ResponseArray { get; set; }
    }

    /// <summary>
    /// Error payload returned on non-200 responses.
    /// </summary>
    public class ErrorResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Client
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Client for the IPWho API.
    ///
    /// Usage:
    /// <code>
    /// var client = new IpWhoClient("sk.xxxx");
    /// var resp = await client.LookupAsync("8.8.8.8");
    /// Console.WriteLine(resp.Data?.GeoLocation?.City);
    /// </code>
    /// </summary>
    public class IpWhoClient : IDisposable
    {
        private const string DefaultBaseUrl = "https://api.ipwho.org";

        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Creates a new IPWho API client.
        /// </summary>
        /// <param name="apiKey">Your IPWho API key (required).</param>
        /// <param name="baseUrl">Override the API base URL (default https://api.ipwho.org).</param>
        /// <param name="httpClient">Optional pre-configured HttpClient (a default is created if null).</param>
        public IpWhoClient(string apiKey, string? baseUrl = null, HttpClient? httpClient = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("apiKey is required", nameof(apiKey));

            _apiKey = apiKey;
            _baseUrl = (baseUrl ?? DefaultBaseUrl).TrimEnd('/');
            _httpClient = httpClient ?? new HttpClient();

            // Default headers
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"ipwho-csharp-sdk/{VersionInfo.Version}");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        // ── Public API ──────────────────────────────────────────────────

        /// <summary>
        /// Look up geolocation for a specific IPv4 or IPv6 address.
        /// </summary>
        /// <param name="ip">The IP address.</param>
        /// <param name="format">Response format: "json" (default), "xml", "csv".</param>
        /// <param name="fields">Comma-separated list of specific fields/objects to include, or null for all.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>IpGeoResponse with the lookup result.</returns>
        public async Task<IpGeoResponse> LookupAsync(
            string ip,
            string? format = null,
            string? fields = null,
            CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"/ip/{ip}", format, fields);
            return await ExecuteAsync(url, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Look up geolocation for the caller's own IP address.
        /// </summary>
        /// <param name="format">Response format: "json" (default), "xml", "csv".</param>
        /// <param name="fields">Comma-separated list of specific fields/objects to include, or null for all.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>IpGeoResponse with the caller's geolocation.</returns>
        public async Task<IpGeoResponse> MeAsync(
            string? format = null,
            string? fields = null,
            CancellationToken cancellationToken = default)
        {
            var url = BuildUrl("/me", format, fields);
            return await ExecuteAsync(url, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform geolocation lookups for multiple IP addresses in a single request.
        /// </summary>
        /// <param name="ips">List of IPv4 or IPv6 addresses.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>List of IpGeoResponse, one per IP in the same order.</returns>
        public async Task<List<IpGeoResponse>> BulkAsync(
            List<string> ips,
            CancellationToken cancellationToken = default)
        {
            if (ips == null || ips.Count == 0)
                throw new ArgumentException("ips must not be empty", nameof(ips));

            var bulkParam = string.Join(",", ips);
            var url = BuildUrl($"/bulk/{bulkParam}", null, null);

            using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var statusCode = (int)response.StatusCode;

            if (statusCode != 200)
                throw MapError(statusCode, body);

            var bulkResp = JsonSerializer.Deserialize<BulkResponse>(body, _jsonOptions);
            if (bulkResp == null || !bulkResp.Success)
                throw new IpWhoException(statusCode, "API returned success=false for bulk request");
            if (bulkResp.Data?.ResponseArray == null)
                throw new IpWhoException(statusCode, "Bulk response data is null");

            return bulkResp.Data.ResponseArray;
        }

        // ── Internal ────────────────────────────────────────────────────

        private string BuildUrl(string path, string? format, string? fields)
        {
            var uriBuilder = new UriBuilder(_baseUrl + path);
            var queryParts = new List<string>();
            queryParts.Add($"apiKey={Uri.EscapeDataString(_apiKey)}");

            if (!string.IsNullOrEmpty(format) && format != "json")
                queryParts.Add($"format={Uri.EscapeDataString(format)}");

            if (!string.IsNullOrEmpty(fields))
                queryParts.Add($"get={Uri.EscapeDataString(fields)}");

            uriBuilder.Query = string.Join("&", queryParts);
            return uriBuilder.ToString();
        }

        private async Task<IpGeoResponse> ExecuteAsync(string url, CancellationToken ct)
        {
            using var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var statusCode = (int)response.StatusCode;

            if (statusCode != 200)
                throw MapError(statusCode, body);

            // Non-JSON formats
            var isJson = true;
            var uri = new Uri(url);
            var qs = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in qs)
            {
                var kv = pair.Split('=', 2);
                if (kv.Length == 2 && kv[0] == "format" && kv[1] != "json")
                {
                    isJson = false;
                    break;
                }
            }
            if (!isJson)
            {
                return new IpGeoResponse
                {
                    Success = true,
                    Data = new GeoData { Ip = body }
                };
            }

            var resp = JsonSerializer.Deserialize<IpGeoResponse>(body, _jsonOptions);
            if (resp == null)
                throw new IpWhoException(statusCode, "Failed to parse response");

            if (!resp.Success)
                throw new IpWhoException(statusCode, resp.Message ?? "API returned success=false");

            return resp;
        }

        private static IpWhoException MapError(int statusCode, string body)
        {
            string message = $"HTTP {statusCode}";
            if (!string.IsNullOrEmpty(body))
            {
                try
                {
                    var err = JsonSerializer.Deserialize<ErrorResponse>(body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (err?.Message != null)
                        message = err.Message;
                }
                catch
                {
                    message = body;
                }
            }

            return statusCode switch
            {
                404 => new NotFoundException(statusCode, message),
                429 => new RateLimitException(statusCode, message),
                _ => new IpWhoException(statusCode, message)
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}