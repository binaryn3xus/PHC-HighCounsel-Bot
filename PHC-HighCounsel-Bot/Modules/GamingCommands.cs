namespace PHC_HighCounsel_Bot.Modules;

public class GamingCommands(ILogger<AICommands> logger) : ModuleBase
{
    //[SlashCommand("phc-flare", "Send up a flare!", false, RunMode.Async)]
    //public async Task GamingFlare()
    //{
    //    await DeferAsync();

    //    var response = new StringBuilder();
    //    response.Append("Anyone gaming?");

    //    // Get the role you want to check

    //    var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Admins");

    //    // Get all the users in the role
    //    var usersInRole = role.Members.Cast<SocketGuildUser>();

    //    // Loop through each user and get their status
    //    foreach (var user in usersInRole)
    //    {
    //        var status = user.Status;
    //        var activity = user.Activities.FirstOrDefault();
    //        // Do something with the user's status
    //        // For example, you can print it to the console
    //        response.AppendLine($"{user.Username} - {status} - {activity}");
    //    }

    //    var embed = new EmbedBuilder()
    //        .WithTitle("Sending up a flare!")
    //        .WithDescription(response.ToString())
    //        .WithAuthor(Context.User.Username, Context.User.GetDisplayAvatarUrl())
    //        .WithColor(Colors.Primary);

    //    await FollowupAsync(embed: embed.Build());
    //}
}