namespace PHC_HighCounsel_Bot;

public class MessageHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;

    public MessageHandler(DiscordSocketClient client)
    {
        _client = client;
    }

    public async Task InitializeAsync()
    {
        _client.MessageReceived += HandleMessageReceived;
    }

    private async Task HandleMessageReceived(SocketMessage message)
    {
        // Check if the message contains "Halo"
        if (message.Content.Contains("Halo"))
        {
            //await message.AddReactionAsync(new Emoji("🎮"));
            await message.AddReactionAsync(Emote.Parse("<:laughguy:1244830211229618296>"));
        }
    }
}
