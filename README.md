# Infertus.HttpService

A lightweight, highly configurable `HttpClient` wrapper for REST API consumption, with throttling, retry logic, authentication, and flexible request building.

---

## Features

* **GET/POST/HEAD** requests with automatic JSON deserialization.
* **File download** support to memory or disk.
* **Bearer & Basic authentication**, headers, cookies, and custom `HttpClient` configuration.
* **Throttling & retry logic** built-in. Automatic retries for `429` and `503` with `Retry-After`.
* **Flexible request URL builder** from path + query dictionary.
* **Raw request support** for advanced scenarios.
* **Easy integration with DI and modern C# projects.**


---

## Installation

```xml
<PackageReference Include="Infertus.Http" Version="1.1.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.2" />
```

---

## Setup (DI)

```csharp
var services = new ServiceCollection();
services.AddInfertusHttpService(new HttpClientConfig
{
    BaseAddress = "https://api.example.com",
    UserAgent = "MyApp/1.0",
    Accept = "application/json",
    BearerToken = "token", // or Username/Password for Basic auth
    Cookies = new CookieCollection(),
    CustomHeaders = new Dictionary<string, string> { { "X-App-Version", "1.0" } }
});
var provider = services.BuildServiceProvider();
var httpService = provider.GetService<IHttpService>();
```

---

## Usage Examples

### GET JSON

```csharp
var uri = HttpRequestBuilder.BuildRequestUri("/some.json", new Dictionary<string, string>
{
    { "key", "API_KEY" },
    { "q", "query" },
});
var jsonObject = await httpService.GetJsonAsync<SomeResponse>(uri);
```

### POST JSON

```csharp
var uri = HttpRequestBuilder.BuildRequestUri("/submit.json", new Dictionary<string, string>());
var response = await httpService.PostAsync(uri, new { Name = "Monty", LastName = "Python" });
```

### Download File

```csharp
var file = await httpService.DownloadFileAsync(new Uri("https://example.com/file.pdf"));
await File.WriteAllBytesAsync("file.pdf", file.Content);
```

### Raw Request

```csharp
var request = new HttpRequestMessage(HttpMethod.Get, uri);
request.Headers.Add("X-Custom", "Value");

var response = await httpService.SendAsync(request);
var content = await httpService.SendRequestAsync(request);
```

---

## HttpRequestBuilder

Helper for constructing request URIs:

```csharp
var queryParams = new Dictionary<string, string> { { "query", "something" } };
var uri = HttpRequestBuilder.BuildRequestUri("/some.json", queryParams);
```

* Supports **absolute URLs** or **relative paths**.
* Automatically encodes query parameters.

---

## HttpService Interface

```csharp
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
```

---

