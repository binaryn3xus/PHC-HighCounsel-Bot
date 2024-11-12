namespace PHC_HighCounsel_Bot.Modules;

public class AICommands(OllamaApiClient ollamaApiClient, ILogger<AICommands> logger) : ModuleBase
{
    [SlashCommand("phcai", "Ask the PHC AI Anything!", false, RunMode.Async)]
    public async Task AIPromptRequestAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            await RespondAsync("You need to provide a prompt!");
            return;
        }

        try
        {
            var rules = GetRules(Context);
            await DeferAsync();

            logger.LogInformation("Prompt: {prompt}{rules}", prompt, rules);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var aiResponse = await GetAIResponseAsync(prompt + rules);
            stopwatch.Stop();

            var limitedResponse = LimitResponseLength(aiResponse, prompt.Length);

            logger.LogInformation("Response: {response}", limitedResponse);

            var embed = BuildEmbed(prompt, limitedResponse, stopwatch.ElapsedMilliseconds);
            await FollowupAsync(embed: embed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing the AI prompt.");
            await FollowupAsync("An error occurred while processing your request. Please try again later.");
        }
    }

    private async Task<string> GetAIResponseAsync(string fullPrompt)
    {
        var aiResponse = new StringBuilder();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        await foreach (var stream in ollamaApiClient.GenerateAsync(fullPrompt))
            aiResponse.Append(stream?.Response);

        stopwatch.Stop();
        return aiResponse.ToString().Trim();
    }

    private string LimitResponseLength(string response, int promptLength)
    {
        const int maxDiscordMessageLength = 2000;
        int maxResponseLength = maxDiscordMessageLength - promptLength;

        return response.Length > maxResponseLength
            ? response.Substring(0, maxResponseLength - 1)
            : response;
    }

    private Embed BuildEmbed(string prompt, string response, long elapsedMilliseconds)
    {
        return new EmbedBuilder()
            .WithTitle(prompt.Trim())
            .WithDescription(response)
            .AddField("Model", ollamaApiClient.SelectedModel, true)
            .AddField("Latency", ConvertToReadableTime(elapsedMilliseconds), true)
            .WithAuthor(Context.User.Username, Context.User.GetDisplayAvatarUrl())
            .WithColor(Colors.Primary)
            .Build();
    }

    private string GetRules(SocketInteractionContext context)
    {
        var sb = new StringBuilder()
            .AppendLine() // Separate from the question
            .AppendLine("Here are some rules for your answer:")
            .AppendLine("- Must respond with no more than 1500 characters!")
            .AppendLine("- Please keep answers short, accurate, and to the point.")
            .AppendLine("- Be factual but do not be overly serious unless previously stated.")
            .AppendLine("- You are allowed to use Markdown and Discord Emojis (1 or 2 max per reply)");

        return sb.ToString();
    }

    private string ConvertToReadableTime(long milliseconds)
    {
        return milliseconds >= 1000 ? $"{milliseconds / 1000.0}s" : $"{milliseconds}ms";
    }
}
