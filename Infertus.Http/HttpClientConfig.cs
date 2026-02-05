using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Infertus.Http;

public class HttpClientConfig2
{
    public string? Accept { get; set; }
    public string? Referer { get; set; }
    public string? UserAgent { get; set; }
    public string? BaseAddress { get; set; }
    public string? BearerToken { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }

    public string? Base64Authorization =>
        !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password)
        ? Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Username.Trim()}:{Password.Trim()}"))
        : null; 
}

public class HttpClientConfig
{
    // Basic
    public string? Accept { get; set; }
    public string? Referer { get; set; }
    public string? UserAgent { get; set; }
    public string? BaseAddress { get; set; }

    // Auth
    public string? BearerToken { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }

    // Headers
    public IDictionary<string, string> DefaultHeaders { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    // Cookies
    public CookieContainer? Cookies { get; set; }

    // Networking
    public TimeSpan? Timeout { get; set; }
    public bool EnableCompression { get; set; } = true;
    public IWebProxy? Proxy { get; set; }
    public bool UseProxy { get; set; }

    // Advanced
    public Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool>?
        ServerCertificateCustomValidation
    { get; set; }

    public string? Base64Authorization =>
        !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password)
            ? Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{Username.Trim()}:{Password.Trim()}"))
            : null;
}
