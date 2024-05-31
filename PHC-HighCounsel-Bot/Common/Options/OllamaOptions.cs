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
    public string Model { get; init; }
}
