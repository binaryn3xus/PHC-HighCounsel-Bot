
using Discord.Interactions;
using OllamaSharp;

namespace PHC_HighCounsel_Bot;

public class DiscordBotService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly ILogger<DiscordBotService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly OllamaApiClient _ollamaApiClient;
    private ConversationContext _context = null;

    public DiscordBotService(DiscordSocketClient client, InteractionService interactionService, ILogger<DiscordBotService> logger, IConfiguration configuration, OllamaApiClient ollamaApiClient, IServiceProvider serviceProvider)
    {
        _client = client;
        _logger = logger;
        _configuration = configuration;
        _ollamaApiClient = ollamaApiClient;
        _serviceProvider = serviceProvider;
        _interactionService = interactionService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // setup logging and the ready event
        _client.Log += LogAsync;
        _interactionService.Log += LogAsync;
        _client.Ready += ReadyAsync;

        // Get Token For Bot
        var token = _configuration["discord:token"];
        ArgumentException.ThrowIfNullOrEmpty(token, "Discord bot token is missing.");

        // Login and start the bot
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // we get the CommandHandler class here and call the InitializeAsync method to start things up for the CommandHandler service
        var commandService = _serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync();
        var messageService = _serviceProvider.GetRequiredService<MessageHandler>().InitializeAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Stop the Discord bot
        await _client.StopAsync();
        _logger.LogInformation("Discord bot stopped.");
    }

    private async Task ReadyAsync()
    {
        if (IsDebug())
        {
            // this is where you put the id of the test discord guild
            System.Console.WriteLine($"In debug mode, adding commands to {1016510099310792744}...");
            await _interactionService.RegisterCommandsToGuildAsync(1016510099310792744);
        }
        else
        {
            // this method will add commands globally, but can take around an hour
            await _interactionService.RegisterCommandsGloballyAsync(true);
        }
        Console.WriteLine($"Connected as -> [{_client.CurrentUser}] :)");
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    static bool IsDebug()
    {
#if DEBUG
        return true;
#else
                return false;
#endif
    }

















    //public async Task StartAsync(CancellationToken cancellationToken)
    //{
    //    // Get the bot token from User Secrets
    //    string botToken = "MTI0Mzk4MDYzMDM0NjMwMTU2Mg.Gr58Q-.YcMMvi-U5KY16jr0NHq0-I-EcyXjVOO4vrDtbQ";
    //    ArgumentException.ThrowIfNullOrEmpty(botToken, "Discord bot token is missing.");

    //    // Initialize Ollama API client
    //    await _ollamaApiClient.PullModel("mistral", status => Console.WriteLine($"({status.Percent}%) {status.Status}"));

    //    // Initialize the Discord bot
    //    _client.Ready += Client_Ready;
    //    _client.SlashCommandExecuted += HandleSlashCommandExecuted;
    //    _client.MessageReceived += HandleMessageReceived;
    //    await _client.LoginAsync(TokenType.Bot, botToken);
    //    await _client.StartAsync();

    //    _logger.LogInformation("Discord bot started.");
    //}

    //private async Task HandleMessageReceived(SocketMessage message)
    //{
    //    Console.WriteLine($"From: {message.Author.Username} | Message: {message.Content}");
    //}

    //private async Task HandleSlashCommandExecuted(SocketSlashCommand command)
    //{
    //    Console.WriteLine($"Command: {command.Data.Name}");
    //    switch (command.Data.Name)
    //    {
    //        case "phc-ai":
    //            var prompt = command.Data.Options.First(o => o.Name == "prompt")?.Value?.ToString();
    //            if (prompt == null)
    //            {
    //                await command.RespondAsync("You need to provide a prompt!");
    //                return;
    //            }

    //            await command.DeferAsync();
    //            var response = new StringBuilder();
    //            response.AppendLine($"{command.User.Username} prompted `{prompt}`").AppendLine();
    //            await _ollamaApiClient.StreamCompletion(prompt, _context, async stream => response.Append(stream.Response));
    //            Console.WriteLine(response.ToString() ?? "Nothing to print");
    //            await command.ModifyOriginalResponseAsync(o => o.Content = response.ToString());
    //            break;
    //    }
    //}

    //private async Task Client_Ready()
    //{
    //    // Let's do our global command
    //    var globalCommand = new SlashCommandBuilder();
    //    globalCommand.WithName("phc-ai");
    //    globalCommand.AddOption("prompt", ApplicationCommandOptionType.String, "The user's question/prompt to the AI", isRequired: true);
    //    globalCommand.WithDescription("Interact with the PHC AI");

    //    try
    //    {
    //        // With global commands we don't need the guild.
    //        await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
    //    }
    //    catch (ApplicationCommandException exception)
    //    {
    //        // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
    //        var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

    //        // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
    //        Console.WriteLine(json);
    //    }
    //}

    //public async Task StopAsync(CancellationToken cancellationToken)
    //{
    //    // Stop the Discord bot
    //    await _client.StopAsync();

    //    _logger.LogInformation("Discord bot stopped.");
    //}
}
