namespace Infertus.Http;

public interface IHttpService
{
    Task<string> GetAsync(string query);
    Task<T> GetJsonAsync<T>(string query);
    Task<HttpResponseMessage> HeadAsync(Uri uri);
    Task<string> PostAsync<TPayload>(string query, TPayload payload);
    Task<string> PostAsJsonAsync<TPayload>(string query, TPayload payload);
    Task<string> GetPageContentStringAsync(Uri uri);
    Task<DownloadedFile> DownloadFileAsync(Uri uri);
    Task<FileInfo> DownloadFileToPathAsync(Uri uri, string targetPath);
}
