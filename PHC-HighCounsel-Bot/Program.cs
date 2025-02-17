﻿
Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .WriteTo.Console()
    .CreateLogger();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(prefix: "Bot_")
    .AddUserSecrets<Program>()
    .Build();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<LinksOptions>().BindConfiguration(LinksOptions.Links).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<DiscordOptions>().BindConfiguration(DiscordOptions.Discord).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<OllamaOptions>().BindConfiguration(OllamaOptions.Ollama).ValidateDataAnnotations().ValidateOnStart();

var discordToken = configuration["Discord:Token"];
var ollamaServer = configuration["Ollama:Server"];
var ollamaModel = configuration["Ollama:Model"];

builder.Services.AddSerilog();
builder.Services.AddSingleton<IConfiguration>(configuration);
builder.Services.AddSingleton(new OllamaApiClient(new Uri(ollamaServer)) { SelectedModel = ollamaModel });

builder.Services.AddDiscordHost((config, _) =>
{
    config.SocketConfig = new DiscordSocketConfig
    {
        LogLevel = LogSeverity.Info,
        //GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildPresences,
        GatewayIntents = GatewayIntents.All,
        LogGatewayIntentWarnings = true,
        UseInteractionSnowflakeDate = false,
        AlwaysDownloadUsers = false,
    };

    config.Token = discordToken!;
});

builder.Services.AddInteractionService((config, _) =>
{
    config.LogLevel = LogSeverity.Debug;
    config.DefaultRunMode = RunMode.Async;
    config.UseCompiledLambda = true;
});
builder.Services.AddInteractiveService(config =>
{
    config.LogLevel = LogSeverity.Warning;
    config.DefaultTimeout = TimeSpan.FromMinutes(5);
    config.ProcessSinglePagePaginators = true;
});

builder.Services.AddHostedService<InteractionHandler>();
builder.Services.AddHostedService<MessageHandler>();

var host = builder.Build();

if (!string.IsNullOrWhiteSpace(ollamaModel))
{
    var ollama = host.Services.GetRequiredService<OllamaApiClient>();
    await foreach (var status in ollama.PullModelAsync(ollamaModel))
        Console.WriteLine($"{status?.Percent}% {status?.Status}");
}

await host.RunAsync();