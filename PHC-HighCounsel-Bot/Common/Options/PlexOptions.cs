namespace PHC_HighCounsel_Bot.Common.Options;

/// <summary>
/// The Ollama options associated with this application.
/// </summary>
public class PlexOptions
{
    /// <summary>
    /// The path to the configuration section.
    /// </summary>
    public const string Plex = nameof(Plex);

    /// <summary>
    /// Server path for the Plex Server. ie. http://your-plex-server:32400/
    /// </summary>
    public required string Server { get; init; }

    /// <summary>
    /// The token to authenticate with the Plex server.
    /// </summary>
    public required string Token { get; init; }
}

