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

        var rules = GetRules();
        await DeferAsync();
        var response = new StringBuilder();
        logger.LogInformation("Prompt: {prompt}{rules}", prompt, rules);
        response.AppendFormat("Prompt: {0}", prompt).AppendLine();
        await ollamaApiClient.StreamCompletion(prompt + rules, null!, stream => response.Append(stream.Response));
        Console.WriteLine(response.ToString());
        await ModifyOriginalResponseAsync(o => o.Content = response.ToString());
    }

    [SlashCommand("phcai-list", "Ask the PHC AI Anything!", false, RunMode.Async)]
    public async Task AIListModels()
    {
        var response = new StringBuilder();
        await DeferAsync();
        var models = await ollamaApiClient.ListLocalModels();
        await ModifyOriginalResponseAsync(o => o.Content = string.Join(',', models.Select(o => o.Name)));
    }

    private string GetRules()
    {
        var sb = new StringBuilder();

        sb.AppendLine(); //Just to separate it from the question
        sb.AppendLine("Here are some rules for your answer:");
        sb.AppendLine("Rule 1: No more than 1800 characters in the response (You are responding in a Discord chat)");
        sb.AppendLine("Rule 2: Be factual but do not be overly serious unless previously stated");

        return sb.ToString();
    }
}

