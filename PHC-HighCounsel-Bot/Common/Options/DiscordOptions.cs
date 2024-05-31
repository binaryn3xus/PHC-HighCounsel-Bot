namespace PHC_HighCounsel_Bot.Common.Options;

/// <summary>
/// The Discord options associated with this application.
/// </summary>
public class DiscordOptions
{
    /// <summary>
    /// The path to the configuration section.
    /// </summary>
    public const string Discord = nameof(Discord);

    /// <summary>
    /// The Discord application token obtained from the https://discord.com/developers/applications.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// The ID of a server in the Discord used for development of this application.
    /// </summary>
    public ulong DevGuildId { get; init; }
}
