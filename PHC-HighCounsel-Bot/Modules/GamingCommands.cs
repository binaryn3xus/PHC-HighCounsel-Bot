namespace PHC_HighCounsel_Bot.Modules;

public class GamingCommands(ILogger<AICommands> logger, IConfiguration configuration) : ModuleBase
{
    [SlashCommand("flare", "Send up a PHC flare!", false, RunMode.Async)]
    public async Task GamingFlare()
    {
        await DeferAsync();

        // Get the role you want to check
        var roleId = configuration.GetValue<ulong>("PHC:MemberRoleId");
        var role = Context.Guild.Roles.FirstOrDefault(r => r.Id == roleId);
        logger.LogTrace(role != null ? $"Sending Flare to Role: {role.Name} (${role.Id})" : "Could not find a role for '${roleId}'", role?.Name, roleId);

        var response = new StringBuilder();
        response.AppendFormat("{0}Anyone gaming?", (role != null) ? $"<@&{role.Id}>" : string.Empty);
        var embed = new EmbedBuilder()
            .WithTitle("Sending up flare!")
            .WithDescription(response.ToString())
            .WithAuthor(Context.User.Username, Context.User.GetDisplayAvatarUrl())
            .WithColor(Colors.Primary);

        await FollowupAsync(embed: embed.Build());
    }
}