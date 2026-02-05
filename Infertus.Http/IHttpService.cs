namespace Infertus.Http;

public interface IHttpService
{
    Task<HttpResponseMessage> HeadAsync(Uri uri);
    Task<string> GetPageContentStringAsync(Uri uri);
    Task<DownloadedFile> DownloadFileAsync(Uri uri);
    Task<FileInfo> DownloadFileToPathAsync(Uri uri, string targetPath);
    Task<T> GetJsonAsync<T>(Uri uri, string? bearerToken = null);
    Task<string> GetAsync(Uri uri, string? bearerToken = null);
    Task<string> PostAsync<TPayload>(Uri uri, TPayload payload, string? bearerToken = null);
    Task<string> SendRequestAsync(HttpRequestMessage request);
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
}
