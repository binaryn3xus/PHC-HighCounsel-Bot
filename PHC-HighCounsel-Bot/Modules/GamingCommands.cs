namespace PHC_HighCounsel_Bot.Modules;

public class GamingCommands(ILogger<GamingCommands> logger, IConfiguration configuration) : ModuleBase
{
    private List<string>? _flarePhrases;

    [SlashCommand("flare", "Send up a PHC flare!", false, RunMode.Async)]
    public async Task GamingFlare()
    {
        await DeferAsync();

        // Retrieve the role ID from configuration
        var roleIdString = configuration.GetValue<string>("PHC:MemberRoleId");
        if (!ulong.TryParse(roleIdString, out ulong roleId))
        {
            logger.LogError("Failed to convert configuration value 'PHC:MemberRoleId' to type 'ulong'.");
            await FollowupAsync("Configuration error: Member role ID is invalid.", ephemeral: true);
            return;
        }

        var role = Context.Guild.Roles.FirstOrDefault(r => r.Id == roleId);
        if (role == null)
        {
            logger.LogWarning("Role with ID {RoleId} not found in guild {GuildId}.", roleId, Context.Guild.Id);
            await FollowupAsync("Role not found in this server. Please check the configuration.", ephemeral: true);
            return;
        }

        logger.LogInformation("Sending Flare to Role: {RoleName} ({RoleId})", role.Name, role.Id);

        var response = new StringBuilder()
            .Append($"<@&{role.Id}> ")
            .Append(GetRandomFlarePhrase());

        var embed = new EmbedBuilder()
            .WithTitle("Sending up a flare!")
            .WithDescription(response.ToString())
            .WithAuthor(Context.User.Username, Context.User.GetDisplayAvatarUrl())
            .WithColor(Colors.Primary)
            .Build();

        await FollowupAsync(embed: embed);
    }

    private string GetRandomFlarePhrase()
    {
        // Lazy-load flare phrases on first access
        if (_flarePhrases == null)
        {
            _flarePhrases = configuration.GetSection("FlarePhrases").Get<List<string>>() ?? new List<string>();

            if (_flarePhrases.Count == 0)
            {
                _flarePhrases.Add("Anyone trying to game?");
                logger.LogWarning("FlarePhrases section is empty or missing. Using default message.");
            }
        }

        var random = new Random();
        return _flarePhrases[random.Next(_flarePhrases.Count)];
    }
}
