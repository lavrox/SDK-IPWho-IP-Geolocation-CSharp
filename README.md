# IPWho ([ipwho.org](https://www.ipwho.org)) C# SDK

[![NuGet](https://img.shields.io/nuget/v/IPWho.Sdk?style=flat-square)](https://www.nuget.org/packages/IPWho.Sdk) [![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg)](https://dotnet.microsoft.com/) [![license](https://img.shields.io/badge/license-MIT-green.svg)](https://github.com/lavrox/SDK-IPWho-IP-Geolocation-CSharp/blob/main/LICENSE)

Official C# / .NET SDK for the [IPWho](https://www.ipwho.org) **IP geolocation API** — geoip lookup, IP location, IP to country / latitude / longitude, ASN/ISP, timezone, currency, flag, and proxy/VPN detection. Works as an IP lookup / ip-geolocation client for IPv4 and IPv6 (`LookupAsync`, `MeAsync`, `BulkAsync`).

- Product: [ipwho.org](https://www.ipwho.org)
- API docs: [ipwho.org/docs](https://www.ipwho.org/docs)
- Get an API key: [ipwho.org/free-plan](https://www.ipwho.org/free-plan) (free [Lavrox](https://lavrox.com) account)
- Live API host: `https://api.ipwho.org`

## API key

Open a free [Lavrox](https://lavrox.com) account to get an API key for [IPWho](https://www.ipwho.org). Create your key at [ipwho.org/free-plan](https://www.ipwho.org/free-plan) — no credit card required.

## Installation

```bash
dotnet add package IPWho.Sdk
```

Targets .NET 8. Namespace: `IPWho`.

## Quick Start

```csharp
using IPWho;

var client = new IpWhoClient(Environment.GetEnvironmentVariable("IPWHO_API_KEY")!);

var resp = await client.LookupAsync("8.8.8.8");   // GET /ip/{ip}
var me = await client.MeAsync();                  // GET /me
var bulk = await client.BulkAsync(new List<string> { "8.8.8.8", "1.1.1.1" });
```

```
IpGeoResponse
├── Success
├── Message
└── Data  GeoData
    ├── Ip
    ├── GeoLocation
    ├── Timezone
    ├── Flag
    ├── Currency
    ├── Connection
    ├── Security
    └── UserAgent
```

`BulkAsync` returns `List<IpGeoResponse>` (not a wrapper).

## Reading the full response (8.8.8.8)

Live [IPWho](https://www.ipwho.org) values: United States, ASN 15169, America/Chicago, dial code +1.

```csharp
var data = (await client.LookupAsync("8.8.8.8")).Data!;
Console.WriteLine(data.Ip); // 8.8.8.8

var geo = data.GeoLocation!;
Console.WriteLine(geo.Country);       // United States
Console.WriteLine(geo.CountryCode);   // US
Console.WriteLine(geo.Continent);
Console.WriteLine(geo.ContinentCode);
Console.WriteLine(geo.Capital);
Console.WriteLine(geo.Region);
Console.WriteLine(geo.RegionCode);
Console.WriteLine(geo.City);
Console.WriteLine(geo.PostalCode);
Console.WriteLine(geo.DialCode);      // +1
Console.WriteLine(geo.IsInEu);
Console.WriteLine(geo.Latitude);
Console.WriteLine(geo.Longitude);
Console.WriteLine(geo.AccuracyRadius);

var tz = data.Timezone!;
Console.WriteLine(tz.TimeZone); // America/Chicago
Console.WriteLine(tz.Abbr);
Console.WriteLine(tz.Offset);
Console.WriteLine(tz.IsDst);
Console.WriteLine(tz.Utc);
Console.WriteLine(tz.CurrentTime);

Console.WriteLine(data.Flag!.FlagIcon);    // 🇺🇸
Console.WriteLine(data.Flag.FlagUnicode);  // U+1F1FA U+1F1F8

Console.WriteLine(data.Currency!.Code);
Console.WriteLine(data.Currency.NamePlural); // US dollars

var conn = data.Connection!;
Console.WriteLine(conn.AsnNumber);      // 15169
Console.WriteLine(conn.AsnOrg);         // Google LLC
Console.WriteLine(conn.Isp);
Console.WriteLine(conn.Org);
Console.WriteLine(conn.Domain);
Console.WriteLine(conn.ConnectionType); // Corporate

Console.WriteLine(data.Security!.IsVpn);
Console.WriteLine(data.Security.IsTor);
Console.WriteLine(data.Security.IsThreat);

if (data.UserAgent?.Browser != null)
    Console.WriteLine(data.UserAgent.Browser.Name);

Console.WriteLine((await client.MeAsync()).Data?.Ip);

foreach (var item in await client.BulkAsync(new List<string> { "8.8.8.8", "1.1.1.1" }))
    Console.WriteLine(item.Data?.Ip);
```

Optional `format` / `fields` / `CancellationToken` on `LookupAsync` and `MeAsync`. Dispose `IpWhoClient` when you own the `HttpClient`.

## API Reference

### `new IpWhoClient(string apiKey, string? baseUrl = null, HttpClient? httpClient = null)`

Query `apiKey`. Default host `https://api.ipwho.org`.

### Errors

`NotFoundException` (404), `RateLimitException` (429), `IpWhoException` (`StatusCode`).

## Type Definitions

```csharp
public class GeoLocation {
    public string? Continent { get; set; }
    public string? ContinentCode { get; set; }
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public string? Capital { get; set; }
    public string? Region { get; set; }
    public string? RegionCode { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? DialCode { get; set; }
    public bool? IsInEu { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? AccuracyRadius { get; set; }
}
```

Also `Timezone` (`TimeZone`, `Abbr`, `Offset`, `IsDst`, `Utc`, `CurrentTime`), `Flag`, `Currency` (`NamePlural`, …), `Connection` (`AsnNumber`, `AsnOrg`, `ConnectionType`, …), `Security` (`IsVpn`, `IsTor`, `IsThreat`), `UserAgent` (`Browser`, `Engine`, `OS`, `Device`, `CPU`).

JSON names follow the live wire (`postal_Code`, `flag_Icon`, `isVpn`).

## Troubleshooting

- Key: [ipwho.org](https://www.ipwho.org).
- HTTP 403: SDK sends `ipwho-csharp-sdk/1.0.0`.
- Use `?.` — city and user-agent are often null on anycast IPs.

## Testing

```bash
IPWHO_API_KEY=your_key dotnet run --project examples/Smoke
```

The live check is `examples/Smoke`.

## Changelog

### v1.0.0

- `LookupAsync`, `MeAsync`, `BulkAsync` matching [api.ipwho.org](https://api.ipwho.org)

## License

MIT License — see [LICENSE](LICENSE).

## Support

- Documentation: [ipwho.org/docs](https://www.ipwho.org/docs)
- Contact: [ipwho.org/contact](https://www.ipwho.org/contact)
- GitHub Issues: [lavrox/SDK-IPWho-IP-Geolocation-CSharp](https://github.com/lavrox/SDK-IPWho-IP-Geolocation-CSharp/issues)
- Website: [ipwho.org](https://www.ipwho.org)

---

[IPWho](https://www.ipwho.org) — a [Lavrox](https://lavrox.com) network API.

[Lavrox](https://lavrox.com) — Independent API infrastructure. Lower latency, lower cost.
