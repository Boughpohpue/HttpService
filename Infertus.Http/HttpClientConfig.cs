using System.Text;

namespace Infertus.Http;

public class HttpClientConfig
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
