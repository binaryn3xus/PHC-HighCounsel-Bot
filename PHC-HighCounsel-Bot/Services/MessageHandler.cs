using System.Text.RegularExpressions;

namespace PHC_HighCounsel_Bot.Services;

internal sealed class MessageHandler(DiscordSocketClient client, ILogger<InteractionHandler> logger, IOptions<DiscordOptions> options, IConfiguration configuration) : DiscordClientService(client, logger)
{
    private readonly ulong _devGuildId = options.Value.DevGuildId;
    private readonly Dictionary<Emote, List<string>> emoteMap = Emotes.GetEmoteDictionary();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.MessageReceived += MessageReceived;
        return Task.CompletedTask;
    }

    private async Task MessageReceived(SocketMessage arg)
    {
        // Ignore bot messages and messages that don't start with '!'
        if (arg is not SocketUserMessage message || message.Author.IsBot || message.Content.StartsWith('!'))
            return;

        // Define the domain replacements
        var domainReplacements = new Dictionary<string, string>
        {
            { "twitter.com", "fxtwitter.com" },
            { "x.com", "fixupx.com" },
            { "bsky.app", "fxbsky.app" }
        };

        bool enableFxLinks = configuration.GetValue<bool>("PHC:EnableFxLinks");
        if (enableFxLinks)
        {
            // Check if the message contains a link from the specified websites
            foreach (var (originalDomain, replacementDomain) in domainReplacements)
            {
                if (message.Content.Contains(originalDomain, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        // Replace the link(s) in the message while keeping the rest of the content
                        var modifiedMessage = ModifyLinksInMessage(message.Content, originalDomain, replacementDomain);

                        // Prepend the message with the user's mention
                        var finalMessage = $"<@{message.Author.Id}> said: {modifiedMessage}";

                        // Delete the original message
                        await message.DeleteAsync();

                        // Post the modified message
                        await message.Channel.SendMessageAsync(finalMessage);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Failed to process the message containing a link.");
                    }

                    // Exit the loop after processing the first matching domain
                    break;
                }
            }
        }

        // Auto reactions based on the words in the message
        bool enableAutoReactions = configuration.GetValue<bool>("PHC:AutoReactions");
        if (enableAutoReactions)
            await AutoReaction(message);
    }

    private string ModifyLinksInMessage(string originalMessage, string originalDomain, string replacementDomain)
    {
        // Use a regex to find and replace all occurrences of the domain in the message
        var regex = new Regex($@"https?://{originalDomain}(\S*)");
        return regex.Replace(originalMessage, match =>
        {
            var linkSuffix = match.Groups[1].Value; // Capture everything after the domain
            return $"https://{replacementDomain}{linkSuffix}";
        });
    }

    private async Task AutoReaction(SocketUserMessage message)
    {
        // Check if the message contains any of the words/phrases in the emoteMap (value) and react with the corresponding emote (key)
        foreach (var (emote, words) in emoteMap)
        {
            if (words.Any(word => message.Content.Contains(word, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    await message.AddReactionAsync(emote);
                }
                catch (HttpException ex) when (ex.Message.Contains("Unknown Emoji"))
                {
                    Logger.LogError(ex, "Failed to add '{EmoteName}' reaction to message.", emote.Name);
                }
            }
        }
    }
}