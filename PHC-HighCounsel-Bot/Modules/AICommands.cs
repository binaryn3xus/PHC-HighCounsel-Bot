using OllamaSharp.Models;

namespace PHC_HighCounsel_Bot.Modules;

public class AICommands(
    OllamaApiClient ollamaApiClient,
    ILogger<AICommands> logger,
    IOptions<OllamaOptions> ollamaOptions) : ModuleBase
{
    private OllamaOptions OllamaConfig => ollamaOptions.Value;

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
            await DeferAsync();

            var formattedPrompt = FormatPrompt(prompt);
            logger.LogInformation("Prompt: {prompt}", formattedPrompt);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var aiResponse = await GetAIResponseAsync(formattedPrompt);
            stopwatch.Stop();

            var limitedResponse = aiResponse.ToString().Length > 2000 ? aiResponse.ToString()[..2000] : aiResponse.ToString();

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

    private string FormatPrompt(string userPrompt)
    {
        return string.Format(OllamaConfig.SystemPrompt, userPrompt);
    }

    private async Task<string> GetAIResponseAsync(string fullPrompt)
    {
        var aiResponse = new StringBuilder();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var request = new GenerateRequest
        {
            Model = ollamaApiClient.SelectedModel,
            Prompt = fullPrompt,
            Stream = true,
            Options = new OllamaSharp.Models.RequestOptions
            {
                Temperature = 0.7f,
                TopP = 0.9f,
                TopK = 40,
                NumPredict = 1000,
                Stop = ["\n\n", "User:", "Assistant:"],
                Seed = Random.Shared.Next()
            }
        };

        try
        {
            await foreach (var stream in ollamaApiClient.GenerateAsync(request))
            {
                if (stream?.Response != null)
                {
                    aiResponse.Append(stream.Response);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ollama API error occurred while generating response");
            throw;
        }

        stopwatch.Stop();
        return aiResponse.ToString().Trim();
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

    private string ConvertToReadableTime(long milliseconds)
    {
        return milliseconds >= 1000 ? $"{milliseconds / 1000.0}s" : $"{milliseconds}ms";
    }
}
