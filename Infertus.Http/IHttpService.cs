namespace Infertus.Http;

public interface IHttpService
{
    Task<HttpResponseMessage> HeadAsync(Uri uri);
    Task<string> GetPageContentStringAsync(Uri uri);
    Task<DownloadedFile> DownloadFileAsync(Uri uri);
    Task<FileInfo> DownloadFileToPathAsync(Uri uri, string targetPath);
    Task<T> GetJsonAsync<T>(string query, string? bearerToken = null);
    Task<string> GetAsync(string query, string? bearerToken = null);
    Task<string> PostAsync<TPayload>(string query, TPayload payload, string? bearerToken = null);
}
