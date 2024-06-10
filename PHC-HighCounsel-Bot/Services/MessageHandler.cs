namespace PHC_HighCounsel_Bot.Services;

internal sealed class MessageHandler(DiscordSocketClient client, ILogger<InteractionHandler> logger, IOptions<DiscordOptions> options) : DiscordClientService(client, logger)
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