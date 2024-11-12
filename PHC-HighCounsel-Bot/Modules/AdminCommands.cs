namespace PHC_HighCounsel_Bot.Modules;

public class AdminCommands(IOptions<LinksOptions> options, OllamaApiClient ollamaApiClient) : ModuleBase
{
    protected LinksOptions Links => options.Value;

    [SlashCommand("about", "Shows information about the app.")]
    public async Task AboutAsync()
    {
        await DeferAsync(ephemeral: true);

        var app = await Context.Client.GetApplicationInfoAsync();

        var localModels = await ollamaApiClient.ListLocalModels();
        var models = string.Join(",\n", localModels.Select(m => m.Name));

        var description = new StringBuilder()
            .AppendLine(app.Description).AppendLine()
            .AppendLine("**AI Models**: \n" + models)
            .ToString();

        var embed = new EmbedBuilder()
            .WithTitle(app.Name)
            .WithDescription(description)
            .AddField("Servers", Context.Client.Guilds.Count, true)
            .AddField("Latency", Context.Client.Latency + "ms", true)
            .WithAuthor(app.Owner.Username, app.Owner.GetDisplayAvatarUrl())
            .WithColor(Colors.Primary)
            .Build();

        var components = new ComponentBuilder()
            //.WithLink("Support", Emotes.Logos.Discord, Links.SupportServerUrl)
            .WithLink("Source", Emotes.Standard.Github, Links.SourceRepositoryUrl)
            .Build();

        await FollowupAsync(embed: embed, components: components, ephemeral: true);
    }
}
