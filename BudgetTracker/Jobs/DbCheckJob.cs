using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace BudgetTracker.Jobs;

public class DbCheckJob
{
    public const string JobId = "db-check";
    public const string CronExpression = "*/10 * * * *";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServer _server;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DbCheckJob> _logger;

    public DbCheckJob(
        IHttpClientFactory httpClientFactory,
        IServer server,
        IConfiguration configuration,
        ILogger<DbCheckJob> logger)
    {
        _httpClientFactory = httpClientFactory;
        _server = server;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var url = ResolveCheckUrl();
        var client = _httpClientFactory.CreateClient(nameof(DbCheckJob));
        var response = await client.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Check de DB fallido. Url: {Url}. Status: {Status}. Body: {Body}",
                url,
                (int)response.StatusCode,
                body);
            throw new InvalidOperationException($"Check de DB fallido ({(int)response.StatusCode}): {body}");
        }

        _logger.LogInformation("Check de DB OK. Url: {Url}", url);
    }

    private string ResolveCheckUrl()
    {
        var configured = _configuration["App:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
            return $"{configured.TrimEnd('/')}/check";

        var addresses = _server.Features.Get<IServerAddressesFeature>()?.Addresses;
        var selected = addresses?
            .OrderBy(address => address.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(selected))
        {
            var port = Environment.GetEnvironmentVariable("PORT") ?? "5036";
            selected = $"http://127.0.0.1:{port}";
        }

        selected = selected
            .Replace("://*", "://127.0.0.1", StringComparison.Ordinal)
            .Replace("://+", "://127.0.0.1", StringComparison.Ordinal);

        var uri = new Uri(selected);
        var host = uri.Host;
        if (host is "0.0.0.0" or "::" or "[::]" or "*")
            host = "127.0.0.1";

        var builder = new UriBuilder(uri) { Host = host };
        return $"{builder.Uri.GetLeftPart(UriPartial.Authority)}/check";
    }

    public static bool AllowLoopbackCertificate(
        HttpRequestMessage request,
        X509Certificate2? certificate,
        X509Chain? chain,
        SslPolicyErrors errors)
    {
        if (errors == SslPolicyErrors.None)
            return true;

        var host = request.RequestUri?.Host;
        return host is "localhost" or "127.0.0.1" or "::1"
            || (IPAddress.TryParse(host, out var ip) && IPAddress.IsLoopback(ip));
    }
}
