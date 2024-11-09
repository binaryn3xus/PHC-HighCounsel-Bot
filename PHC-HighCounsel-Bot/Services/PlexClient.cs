using PHC_HighCounsel_Bot.Handlers;
using System.Xml;

namespace PHC_HighCounsel_Bot.Services;

public class PlexClient
{
    private readonly ILogger<PlexClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly PlexOptions _plexOptions;

    public PlexClient(IOptions<PlexOptions> plexOptions, ILogger<PlexClient> logger)
    {
        _logger = logger;
        _plexOptions = plexOptions.Value;
        var handler = new PlexHttpClientHandler(_plexOptions.Token) { InnerHandler = new HttpClientHandler() };
        _httpClient = new HttpClient(handler);
    }

    public string Token => _plexOptions.Token;

    public async Task<string> GetLibrarySectionKeyAsync(string libraryName)
    {
        var url = $"{_plexOptions.Server}/library/sections";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return ExtractLibrarySectionKey(content, libraryName);
    }

    private string ExtractLibrarySectionKey(string content, string libraryName)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(content);
        var nodes = xmlDoc.SelectNodes("/MediaContainer/Directory");
        foreach (XmlNode node in nodes)
        {
            if (node.Attributes["title"].Value == libraryName)
            {
                return node.Attributes["key"].Value;
            }
        }
        throw new Exception($"Library section with name '{libraryName}' not found.");
    }

    public async Task<List<string>> GetAllTrackIdsAsync(string librarySectionKey)
    {
        var trackIds = new List<string>();
        var artistsUrl = $"{_plexOptions.Server}/library/sections/{librarySectionKey}/all";
        var artistsResponse = await _httpClient.GetAsync(artistsUrl);
        artistsResponse.EnsureSuccessStatusCode();
        var artistsContent = await artistsResponse.Content.ReadAsStringAsync();
        var artistIds = ExtractArtistIds(artistsContent);

        foreach (var artistId in artistIds)
        {
            var albumsUrl = $"{_plexOptions.Server}/library/metadata/{artistId}/children";
            var albumsResponse = await _httpClient.GetAsync(albumsUrl);
            albumsResponse.EnsureSuccessStatusCode();
            var albumsContent = await albumsResponse.Content.ReadAsStringAsync();
            var albumIds = ExtractAlbumIds(albumsContent);

            foreach (var albumId in albumIds)
            {
                var tracksUrl = $"{_plexOptions.Server}/library/metadata/{albumId}/children";
                var tracksResponse = await _httpClient.GetAsync(tracksUrl);
                tracksResponse.EnsureSuccessStatusCode();
                var tracksContent = await tracksResponse.Content.ReadAsStringAsync();
                trackIds.AddRange(ExtractTrackIds(tracksContent));
            }
        }
        return trackIds;
    }

    public async Task<string> DownloadAndSaveAsMp3Async(string streamUrl)
    {
        try
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp3");

            using (var stream = await _httpClient.GetStreamAsync(streamUrl))
            using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                byte[] buffer = new byte[81920];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                }
            }

            return tempPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading and saving the stream: {ex.Message}");
            throw;
        }
    }


    private List<string> ExtractArtistIds(string content)
    {
        var artistIds = new List<string>();
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(content);
        var nodes = xmlDoc.SelectNodes("/MediaContainer/Directory");
        foreach (XmlNode node in nodes)
        {
            artistIds.Add(node.Attributes["ratingKey"].Value);
        }
        return artistIds;
    }

    private List<string> ExtractAlbumIds(string content)
    {
        var albumIds = new List<string>();
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(content);
        var nodes = xmlDoc.SelectNodes("/MediaContainer/Directory");
        foreach (XmlNode node in nodes)
        {
            albumIds.Add(node.Attributes["ratingKey"].Value);
        }
        return albumIds;
    }

    private List<string> ExtractTrackIds(string content)
    {
        var trackIds = new List<string>();
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(content);
        var nodes = xmlDoc.SelectNodes("/MediaContainer/Track");
        foreach (XmlNode node in nodes)
        {
            trackIds.Add(node.Attributes["ratingKey"].Value);
            _logger.LogTrace("Adding track id: {trackId} | '{title}' by '{artist}'", 
                node.Attributes["ratingKey"].Value, node.Attributes["title"].Value, node.Attributes["grandparentTitle"].Value);
        }
        return trackIds;
    }

    public async Task<string> GetAudioStreamUrlAsync(long trackId)
    {
        var url = $"{_plexOptions.Server}/library/metadata/{trackId}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var xmlContent = await response.Content.ReadAsStringAsync();

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlContent);
        var mediaPartNodes = xmlDoc.SelectNodes("/MediaContainer/Track/Media/Part");

        if (mediaPartNodes != null && mediaPartNodes.Count > 0)
        {
            // Pick the first media part for simplicity, adjust as needed
            var mediaPartNode = mediaPartNodes[0];
            var mediaUrl = mediaPartNode.Attributes["key"].Value;

            return $"{_plexOptions.Server}{mediaUrl}?X-Plex-Token={_plexOptions.Token}";
        }

        throw new Exception($"No media parts found for track ID '{trackId}'");
    }

}
