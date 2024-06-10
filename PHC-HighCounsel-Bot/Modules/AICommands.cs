namespace PHC_HighCounsel_Bot.Modules;

public class AICommands(OllamaApiClient ollamaApiClient, ILogger<AICommands> logger) : ModuleBase
{
    [SlashCommand("phcai", "Ask the PHC AI Anything!", false, RunMode.Async)]
    public async Task AIPromptRequestAsync(string prompt)
    {
        if (prompt == null)
        {
            await RespondAsync("You need to provide a prompt!");
            return;
        }

        var rules = GetRules(Context);
        await DeferAsync();
        logger.LogInformation("Prompt: {prompt}{rules}", prompt, rules);

        var aiResponse = new StringBuilder();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await ollamaApiClient.StreamCompletion(prompt + rules, null!, stream => aiResponse.Append(stream.Response));
        stopwatch.Stop();
        var discordResponse = new StringBuilder();
        discordResponse.AppendLine(aiResponse.ToString().Trim());

        var limitedResponse = (discordResponse.Length > 2000) ? discordResponse.ToString().Substring(0, 1999 - prompt.Length) : discordResponse.ToString();
        logger.LogInformation("Response: {response}", limitedResponse);

        var embed = new EmbedBuilder()
            .WithTitle(prompt.Trim())
            .WithDescription(limitedResponse)
            .AddField("Model", ollamaApiClient.SelectedModel, true)
            .AddField("Latency", ConvertToReadableTime(stopwatch.ElapsedMilliseconds), true)
            .WithAuthor(Context.User.Username, Context.User.GetDisplayAvatarUrl())
            //.WithFooter(string.Join(" · ", app.Tags.Select(t => '#' + t)))
            .WithColor(Colors.Primary)
            .Build();

        await FollowupAsync(embed: embed);
    }

    private string GetRules(SocketInteractionContext context)
    {
        var author = (Context.User as SocketGuildUser);
        var sb = new StringBuilder();
        sb.AppendLine(); //Just to separate it from the question
        sb.AppendLine("Here are some rules for your answer:");
        sb.AppendLine("- Must respond with no more than 1500 characters!");
        sb.AppendLine("- Please keep answers short, accurate, and to-the-point.");
        sb.AppendLine("- Be factual but do not be overly serious unless previously stated");
        sb.AppendLine("- You are allowed to use Markdown and Discord Emojis (1 or 2 max per reply)");

        return sb.ToString();
    }

    private string ConvertToReadableTime(long milliseconds)
    {
        if (milliseconds >= 1000)
        {
            double seconds = milliseconds / 1000.0;
            return $"{seconds}s";
        }
        else
        {
            return $"{milliseconds}ms";
        }
    }
}

