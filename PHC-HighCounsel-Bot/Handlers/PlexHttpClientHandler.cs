namespace PHC_HighCounsel_Bot.Handlers;

public class PlexHttpClientHandler : DelegatingHandler
{
    private readonly string _plexToken;

    public PlexHttpClientHandler(string plexToken)
    {
        _plexToken = plexToken;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uriBuilder = new UriBuilder(request.RequestUri);
        var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
        query["X-Plex-Token"] = _plexToken;
        uriBuilder.Query = query.ToString();
        request.RequestUri = uriBuilder.Uri;

        return base.SendAsync(request, cancellationToken);
    }
}