namespace PHC_HighCounsel_Bot.Modules;

public class MaintenanceCommands(MessageHandler handler) : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService Commands { get; set; }
    private MessageHandler _handler = handler;

#if DEBUG
    [SlashCommand("clean-channel", "Warning: This will delete all messages in this channel!", false, RunMode.Async)]
    [RequireRole("Admins")]
    public async Task ChannelCleanup(bool areYouSure = false)
    {
        if (areYouSure)
        {
            //await DeleteAllMessagesInChannel(message.Channel);
        }
    }
#endif

}