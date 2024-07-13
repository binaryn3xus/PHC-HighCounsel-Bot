namespace PHC_HighCounsel_Bot.Modules;

public class GamingCommands(ILogger<AICommands> logger, IConfiguration configuration) : ModuleBase
{
#if DEBUG
    [SlashCommand("dev-flare", "Ask the PHC AI Anything!", false, RunMode.Async)]
#else
    [SlashCommand("flare", "Send up a PHC flare!", false, RunMode.Async)]
#endif
    public async Task GamingFlare()
    {
        await DeferAsync();

        // Get the role you want to check
        var roleIdString = configuration.GetValue<string>("PHC:MemberRoleId");
        if (!ulong.TryParse(roleIdString, out ulong roleId))
        {
            logger.LogError($"Failed to convert configuration value 'PHC:MemberRoleId' to type 'System.UInt64'");
            return;
        }

        var role = Context.Guild.Roles.FirstOrDefault(r => r.Id == roleId);
        logger.LogTrace(role != null ? $"Sending Flare to Role: {role.Name} (${role.Id})" : $"Could not find a role for '{roleId}'", role?.Name, roleId);

        var response = new StringBuilder();
        response.AppendFormat("{0}{1}", (role != null) ? $"<@&{role.Id}> " : string.Empty, GetRandomFlarePhrase());
        var embed = new EmbedBuilder()
            .WithTitle("Sending up flare!")
            .WithDescription(response.ToString())
            .WithAuthor(Context.User.Username, Context.User.GetDisplayAvatarUrl())
            .WithColor(Colors.Primary);

        await FollowupAsync(embed: embed.Build());
    }

    private string GetRandomFlarePhrase()
    {
        var flarePhrases = new List<string>
        {
            "Where are the bois at?",
            "Any bois ready to game?",
            "Bois, assemble! Where you at?",
            "Calling all bois! Game time, where you hiding?",
            "Looking for the bois! Who's in?",
            "Attention bois: Game session starting, location check?",
            "Alright bois, roll call! Where's everyone?",
            "Bois, report your gaming stations!",
            "Gather 'round bois, it's game o'clock! Where are you?",
            "Bois, sound off! Who's up for some gaming action?"
        };

        var random = new Random();
        var randomIndex = random.Next(0, flarePhrases.Count);
        return flarePhrases[randomIndex];
    }
}
