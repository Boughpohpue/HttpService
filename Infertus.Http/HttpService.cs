using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using CompletionOpt = System.Net.Http.HttpCompletionOption;
using StatusCode = System.Net.HttpStatusCode;

namespace Infertus.Http;

public class HttpService : IHttpService
{
    private const int DelayMilliseconds = 1000;

    private static readonly SemaphoreSlim _throttle = new(1, 1);

    private readonly HttpClient _client;


    public HttpService(HttpClient client)
    {
        _client = client;
    }


    public async Task<HttpResponseMessage> HeadAsync(Uri uri)
    {
        ValidateUri(uri);
        await _throttle.WaitAsync().ConfigureAwait(false);
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Head, uri);
            return await SendWithRetryAsync(() => 
                    _client.SendAsync(request))
                .ConfigureAwait(false);
        }
        finally
        {
            await Task.Delay(DelayMilliseconds)
                .ConfigureAwait(false);
            _throttle.Release();
        }
    }

    public async Task<T> GetJsonAsync<T>(Uri uri, string? bearerToken = null)
    {
        var json = await GetAsync(uri, bearerToken)
            .ConfigureAwait(false);

        return JsonConvert.DeserializeObject<T>(json)!;
    }

    public async Task<string> GetPageContentStringAsync(Uri uri)
    {
        using var response = await SendGetAsync(uri)
            .ConfigureAwait(false);

        return await response.Content.ReadAsStringAsync()
            .ConfigureAwait(false);
    }

    public async Task<DownloadedFile> DownloadFileAsync(Uri uri)
    {
        using var response = await SendGetAsync(uri,
                CompletionOpt.ResponseHeadersRead)
            .ConfigureAwait(false);

        var bytes = await response.Content.ReadAsByteArrayAsync()
            .ConfigureAwait(false);

        var fileName =
            response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName
            ?? Path.GetFileName(uri.LocalPath)
            ?? "download";

        return new DownloadedFile(bytes, fileName.Trim('"'));
    }

    public async Task<FileInfo> DownloadFileToPathAsync(Uri uri, string targetPath)
    {
        var file = await DownloadFileAsync(uri);
        await File.WriteAllBytesAsync(targetPath, file.Content)
            .ConfigureAwait(false);
        return new FileInfo(targetPath);
    }

    public async Task<string> GetAsync(Uri uri, string? bearerToken = null)
    {
        ValidateUri(uri);
        await _throttle.WaitAsync().ConfigureAwait(false);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (bearerToken != null)
                request.Headers.Authorization = 
                    new AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await SendWithRetryAsync(() =>
                    _client.SendAsync(request))
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
        }
        finally
        {
            await Task.Delay(DelayMilliseconds)
                .ConfigureAwait(false);
            _throttle.Release();
        }
    }

    public async Task<string> PostAsync<TPayload>(
        Uri uri, TPayload payload, string? bearerToken = null)
    {
        ValidateUri(uri);
        await _throttle.WaitAsync().ConfigureAwait(false);
        try
        {
            var json = JsonConvert.SerializeObject(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, uri) 
            { Content = content };

            if (bearerToken != null)
                request.Headers.Authorization = 
                    new AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await SendWithRetryAsync(() =>
                    _client.SendAsync(request))
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
        }
        finally
        {
            await Task.Delay(DelayMilliseconds)
                .ConfigureAwait(false);
            _throttle.Release();
        }
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        await _throttle.WaitAsync().ConfigureAwait(false);
        try
        {
            return await SendWithRetryAsync(() => 
                _client.SendAsync(request))
                    .ConfigureAwait(false);
        }
        finally
        {
            await Task.Delay(DelayMilliseconds)
                .ConfigureAwait(false);
            _throttle.Release();
        }
    }

    public async Task<string> SendRequestAsync(HttpRequestMessage request)
    {
        await _throttle.WaitAsync().ConfigureAwait(false);
        try
        {
            var response = await SendAsync(request)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
        }
        finally
        {
            await Task.Delay(DelayMilliseconds)
                .ConfigureAwait(false);
            _throttle.Release();
        }
    }

    private async Task<HttpResponseMessage> SendGetAsync(Uri uri,
        CompletionOpt completionOpt = CompletionOpt.ResponseContentRead)
    {
        ValidateUri(uri);
        await _throttle.WaitAsync().ConfigureAwait(false);
        try
        {
            var response = await SendWithRetryAsync(() =>
                    _client.GetAsync(uri, completionOpt))
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return response;
        }
        finally
        {
            await Task.Delay(DelayMilliseconds)
                .ConfigureAwait(false);
            _throttle.Release();
        }
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(
        Func<Task<HttpResponseMessage>> send, int maxRetries = 1)
    {
        for (int attempt = 0; ; attempt++)
        {
            var response = await send().ConfigureAwait(false);

            if (response.IsSuccessStatusCode || !IsRetryable(response.StatusCode))
                return response;

            if (attempt >= maxRetries)
                return response;

            var delay = GetRetryAfter(response) 
                ?? TimeSpan.FromMilliseconds(DelayMilliseconds);

            await Task.Delay(delay)
                .ConfigureAwait(false);
        }
    }

    private void ValidateUri(Uri uri)
    {
        if (!uri.IsAbsoluteUri && _client.BaseAddress == null)
            throw new ArgumentException($"Can't use relative uri while base address is not set!");
    }

    private static TimeSpan? GetRetryAfter(HttpResponseMessage response)
    {
        if (response.Headers.RetryAfter == null)
            return null;

        if (response.Headers.RetryAfter.Delta.HasValue)
            return response.Headers.RetryAfter.Delta.Value;

        if (response.Headers.RetryAfter.Date.HasValue)
            return response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;

        return null;
    }

    private static bool IsRetryable(StatusCode code) =>
        code is StatusCode.TooManyRequests or StatusCode.ServiceUnavailable;
}
