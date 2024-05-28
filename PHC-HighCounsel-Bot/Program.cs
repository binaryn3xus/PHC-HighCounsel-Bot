Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .WriteTo.Console()
    .CreateLogger();

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var builder = Host.CreateDefaultBuilder(args);

var discordToken = configuration["discord:token"];
var ollamaServer = configuration["ollama:server"];
var ollamaModel = configuration["ollama:model"];

builder.ConfigureServices((hostContext, services) =>
{
    services.AddSingleton<IConfiguration>(configuration);
    services.AddSerilog();
    services.AddSingleton(provider => new DiscordSocketClient(new DiscordSocketConfig() { MessageCacheSize = 200, GatewayIntents = GatewayIntents.All }));
    services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
    services.AddSingleton<ConversationContext>();
    services.AddSingleton<CommandHandler>();
    services.AddSingleton<MessageHandler>();
    services.AddSingleton(new OllamaApiClient(new Uri(ollamaServer)) { SelectedModel = ollamaModel });
    services.AddHostedService<DiscordBotService>();
});

var host = builder.Build();

if(!string.IsNullOrWhiteSpace(ollamaModel))
{
    var ollama = host.Services.GetRequiredService<OllamaApiClient>();
    await ollama.PullModel(ollamaModel, status => Console.WriteLine($"({status.Percent}%) {status.Status}"));
}

await host.RunAsync();