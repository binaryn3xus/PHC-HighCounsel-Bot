using Microsoft.Extensions.Options;

namespace PHC_HighCounsel_Bot.Modules;

public class AdminCommands(IOptions<LinksOptions> options, OllamaApiClient ollamaApiClient) : ModuleBase
{
    protected LinksOptions Links => options.Value;

    [SlashCommand("about", "Shows information about the app.")]
    public async Task AboutAsync()
    {
        await DeferAsync();
        var app = await Context.Client.GetApplicationInfoAsync();

        var localModels = await ollamaApiClient.ListLocalModels();
        var models = string.Join(", ", localModels.Select(m => m.Name));

        var description = new StringBuilder()
            .AppendLine(app.Description)
            .AppendLine()
            .AppendLine("Models: " + models)
            .ToString();

        var embed = new EmbedBuilder()
            .WithTitle(app.Name)
            .WithDescription(description)
            .AddField("Servers", Context.Client.Guilds.Count, true)
            .AddField("Latency", Context.Client.Latency + "ms", true)
            .AddField("Version", Assembly.GetEntryAssembly().GetName().Version, true)
            .WithAuthor(app.Owner.Username, app.Owner.GetDisplayAvatarUrl())
            .WithFooter(string.Join(" · ", app.Tags.Select(t => '#' + t)))
            .WithColor(Colors.Primary)
            .Build();
        
        var components = new ComponentBuilder()
            .WithLink("Support", Emotes.Logos.Discord, Links.SupportServerUrl)
            .WithLink("Source", Emotes.Logos.Github, Links.SourceRepositoryUrl)
            .Build();

        await FollowupAsync(embed: embed, components: components);
    }

//#if DEBUG
//    [SlashCommand("clean-channel", "Warning: This will delete all messages in this channel!", false, RunMode.Async)]
//    [RequireRole("Admins")]
//    public async Task ChannelCleanup(bool areYouSure = false)
//    {
//        if (areYouSure)
//        {
//            //await DeleteAllMessagesInChannel(message.Channel);
//        }
//    }
//#endif

}