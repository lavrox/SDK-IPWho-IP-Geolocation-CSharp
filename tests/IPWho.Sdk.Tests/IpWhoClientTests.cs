using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IPWho;
using Xunit;

namespace IPWho.Sdk.Tests
{
    /// <summary>
    /// A stub handler answers every request with a recorded API response,
    /// so these tests need no API key and no network access.
    /// </summary>
    public class IpWhoClientTests
    {
        private const string Key = "sk.test_secret_value";

        private sealed class StubHandler : HttpMessageHandler
        {
            public HttpStatusCode Status = HttpStatusCode.OK;
            public string Body = Fixture("lookup");
            public readonly List<(string Url, string? Authorization)> Requests = new();

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add((request.RequestUri!.AbsoluteUri, request.Headers.Authorization?.ToString()));
                return Task.FromResult(new HttpResponseMessage(Status)
                {
                    Content = new StringContent(Body, Encoding.UTF8, "application/json")
                });
            }
        }

        private readonly StubHandler _stub = new();
        private readonly HttpClient _http;
        private readonly IpWhoClient _client;

        public IpWhoClientTests()
        {
            _http = new HttpClient(_stub);
            _client = new IpWhoClient(Key, httpClient: _http);
        }

        private static string Fixture(string name) =>
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", name + ".json"));

        [Fact]
        public async Task KeyIsInTheHeaderAndNeverInTheUrl()
        {
            await _client.LookupAsync("8.8.8.8");
            await _client.MeAsync();
            _stub.Body = Fixture("bulk");
            await _client.BulkAsync(new List<string> { "8.8.8.8", "1.1.1.1" });

            Assert.Equal(3, _stub.Requests.Count);
            Assert.All(_stub.Requests, request =>
            {
                Assert.Equal("Bearer " + Key, request.Authorization);
                Assert.DoesNotContain(Key, request.Url);
                Assert.DoesNotContain("apikey", request.Url, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("?", request.Url);
            });
            Assert.Null(_http.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public async Task LookupMapsTheResponse()
        {
            var data = (await _client.LookupAsync("8.8.8.8")).Data!;
            Assert.Equal("8.8.8.8", data.Ip);
            Assert.Equal("United States", data.GeoLocation!.Country);
            Assert.Equal("US", data.GeoLocation.CountryCode);
            Assert.Equal("+1", data.GeoLocation.DialCode);
            Assert.Equal(15169, data.Connection!.AsnNumber);
            Assert.Equal("America/Chicago", data.Timezone!.TimeZone);
            Assert.Equal("USD", data.Currency!.Code);
        }

        [Fact]
        public async Task LookupAcceptsANullCurrency()
        {
            _stub.Body = Fixture("lookup-null-currency");
            var data = (await _client.LookupAsync("1.1.1.1")).Data!;
            Assert.Equal("1.1.1.1", data.Ip);
            Assert.Null(data.Currency);
        }

        [Fact]
        public async Task BulkReturnsTheDataForEveryAddress()
        {
            _stub.Body = Fixture("bulk");
            var ips = new List<string> { "8.8.8.8", "1.1.1.1", "192.168.0.1", "9.9.9.9" };
            var rows = await _client.BulkAsync(ips);

            Assert.Equal(ips, rows.Select(row => row.Data!.Ip));

            Assert.True(rows[0].Success);
            Assert.Equal("United States", rows[0].Data!.GeoLocation!.Country);
            Assert.Equal(15169, rows[0].Data!.Connection!.AsnNumber);

            Assert.True(rows[1].Success);
            Assert.Null(rows[1].Data!.Currency);

            Assert.False(rows[2].Success);
            Assert.Null(rows[2].Data!.GeoLocation);
            Assert.Equal("Reserved range/Invalid IP address", rows[2].Message);

            Assert.False(rows[3].Success);
            Assert.Contains("quota", rows[3].Message);
        }

        [Fact]
        public async Task FieldsAndFormatAreTheOnlyQueryParameters()
        {
            await _client.LookupAsync("8.8.8.8", fields: "geoLocation,timezone");
            Assert.EndsWith("/ip/8.8.8.8?get=geoLocation%2Ctimezone", _stub.Requests[0].Url);
            _stub.Body = "ip,8.8.8.8";
            var response = await _client.LookupAsync("8.8.8.8", format: "csv");
            Assert.Equal("ip,8.8.8.8", response.Data!.Ip);
            Assert.EndsWith("/ip/8.8.8.8?format=csv", _stub.Requests[1].Url);
        }

        [Theory]
        [InlineData(404, "{\"success\":false,\"message\":\"nope\"}", typeof(NotFoundException))]
        [InlineData(429, "{\"error\":\"rate_limit_exceeded\"}", typeof(RateLimitException))]
        [InlineData(401, "{\"error\":\"invalid_api_key\"}", typeof(IpWhoException))]
        public async Task HttpErrorsThrow(int status, string body, Type expected)
        {
            _stub.Status = (HttpStatusCode)status;
            _stub.Body = body;
            var error = await Assert.ThrowsAsync(expected, () => _client.LookupAsync("8.8.8.8"));
            Assert.Equal(status, ((IpWhoException)error).StatusCode);
            Assert.DoesNotContain(Key, error.Message);
            if (status == 401)
                Assert.Equal("invalid_api_key", error.Message);
        }

        [Fact]
        public async Task KeyWithSurroundingWhitespaceIsTrimmed()
        {
            await new IpWhoClient($"  {Key}\n", httpClient: new HttpClient(_stub)).LookupAsync("8.8.8.8");
            Assert.Equal("Bearer " + Key, _stub.Requests[0].Authorization);
        }

        [Fact]
        public void KeyWithInvalidCharactersIsRejectedWithoutRepeatingIt()
        {
            var error = Assert.Throws<ArgumentException>(() => new IpWhoClient("sk.secret_part\nmore"));
            Assert.DoesNotContain("secret_part", error.Message);
            Assert.Throws<ArgumentException>(() => new IpWhoClient("  \n"));
        }
    }
}
