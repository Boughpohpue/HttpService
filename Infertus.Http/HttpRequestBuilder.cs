namespace Infertus.Http;

public static class HttpRequestBuilder
{
    public static string BuildQueryString(Dictionary<string, string> queryParams) =>
        string.Join("&", queryParams.Select(kv =>
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

    public static string BuildRequestUrl(string urlOrPath, Dictionary<string, string> queryParams) =>
        queryParams.Count == 0 ? urlOrPath : $"{urlOrPath}?{BuildQueryString(queryParams)}";

    public static Uri BuildRequestUri(string urlOrPath, Dictionary<string, string> queryParams)
    {
        var requestUrl = BuildRequestUrl(urlOrPath, queryParams);
        if (Uri.TryCreate(requestUrl, UriKind.Absolute, out var absolute))
            return absolute;

        return new Uri(requestUrl.TrimStart('/'), UriKind.Relative);
    }
}
