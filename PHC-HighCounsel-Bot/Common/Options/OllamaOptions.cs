namespace PHC_HighCounsel_Bot.Common.Options;

/// <summary>
/// The Ollama options associated with this application.
/// </summary>
public class OllamaOptions
{
    /// <summary>
    /// The path to the configuration section.
    /// </summary>
    public const string Ollama = nameof(Ollama);

    /// <summary>
    /// Server path for the Ollama API to use. ie. http://localhost:11434
    /// </summary>
    public required string Server { get; init; }

    /// <summary>
    /// The name of the model to use in Ollama
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// The system prompt template to use for AI interactions
    /// </summary>
    public string SystemPrompt { get; init; } = @"You are a helpful AI assistant for the PHC Discord server. Follow these rules:
- Keep responses under 1500 characters
- Be concise and accurate
- Be factual but not overly serious unless specified
- You can use Markdown and up to 2 Discord emojis per reply
- Format your responses in a clear, readable way

User's question: {0}";
}
