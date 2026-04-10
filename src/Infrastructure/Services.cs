using CF_DUC_Tool.src.Core;
using CF_DUC_Tool.src.Core.Exceptions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace CF_DUC_Tool.src.Infrastructure;

public class FailoverIpProvider : IIpProvider
{
    private readonly HttpClient _http;
    // 0xDEAD = 57005ms (~57s) timeout
    private const int TimeToDie = 0xDEAD;

    private readonly string[] _v4Hosts = { "https://api.ipify.org", "https://ipv4.icanhazip.com", "https://checkip.amazonaws.com" };
    private readonly string[] _v6Hosts = { "https://api64.ipify.org", "https://ipv6.icanhazip.com", "https://ifconfig.co/ip" };

    public FailoverIpProvider()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromMilliseconds(TimeToDie) };
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("CF-DUC-Tool/1.0");
    }

    public async Task<string> GetPublicIpAsync(string recordType)
    {
        var hosts = recordType == "AAAA" ? _v6Hosts : _v4Hosts;
        foreach (var host in hosts)
        {
            try { return (await _http.GetStringAsync(host)).Trim(); } catch { continue; }
        }
        throw new NetworkException($"Não foi possível obter IP Público ({recordType}). Todos os serviços falharam.");
    }
}

public class CloudflareDnsProvider : IDnsProvider
{
    private readonly HttpClient _http;

    public CloudflareDnsProvider()
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("CF-DUC-Tool/1.0");
    }

    private HttpRequestMessage CreateReq(HttpMethod method, string url, string token, object? content = null)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (content != null) req.Content = JsonContent.Create(content);
        return req;
    }

    public async Task<(string Id, string Content)?> GetRecordInfoAsync(string apiToken, string zoneId, string recordName, string recordType)
    {
        var url = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records?type={recordType}&name={recordName}";
        var res = await _http.SendAsync(CreateReq(HttpMethod.Get, url, apiToken));

        if (!res.IsSuccessStatusCode) return null;
        var json = await res.Content.ReadFromJsonAsync<JsonNode>();

        var item = json?["result"]?.AsArray().FirstOrDefault();
        return item != null ? (item["id"]?.ToString(), item["content"]?.ToString()) : null;
    }

    public async Task<bool> UpdateRecordAsync(string apiToken, string zoneId, string recordId, string recordName, string newIp, string type, int ttl, bool proxied)
    {
        var url = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records/{recordId}";
        var payload = new { type, name = recordName, content = newIp, ttl, proxied };

        var res = await _http.SendAsync(CreateReq(HttpMethod.Put, url, apiToken, payload));
        var json = await res.Content.ReadFromJsonAsync<JsonNode>();
        return (bool)json?["success"]!;
    }
}