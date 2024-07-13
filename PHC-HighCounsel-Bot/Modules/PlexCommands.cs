using Discord;
using PHC_HighCounsel_Bot.Services;

namespace PHC_HighCounsel_Bot.Modules;

public class PlexCommands(ILogger<PlexCommands> logger, PlexClient plexClient, AudioStreamer audioStreamer) : ModuleBase
{
#if DEBUG
    [SlashCommand("dev-plex", "Start Plex Radio!", false, RunMode.Async)]
#else
    [SlashCommand("plex", "Start Plex Radio!", false, RunMode.Async)]
#endif
    public async Task PlexStart()
    {
        await DeferAsync();
        var user = Context.User as IGuildUser;
        var voiceChannel = user.VoiceChannel;

        if (voiceChannel == null)
        {
            var textChannel = Context.Channel as ITextChannel;
            if (textChannel != null)
            {
                var guild = Context.Guild;
                voiceChannel = guild.VoiceChannels.FirstOrDefault(vc => vc.Name.Equals(textChannel.Name, StringComparison.OrdinalIgnoreCase));
                logger.LogInformation("Voice Channel Found for {slashCommandName}: {voiceChannel}", "/plex", voiceChannel?.Name);
            }
        }

        if (voiceChannel == null)
        {
            var embed = new EmbedBuilder()
                .WithTitle("Error!")
                .WithDescription("You must be connected to a voice channel to use this command.")
                .WithAuthor(Context.User.Username, Context.User.GetDisplayAvatarUrl())
                .WithColor(Colors.Danger);

            await FollowupAsync(embed: embed.Build());
            return;
        }

        //var librarySectionKey = await plexClient.GetLibrarySectionKeyAsync("Music");
        //var trackIds = await plexClient.GetAllTrackIdsAsync(librarySectionKey);

        //Test Track = 37384 // Creed - With Arms Wide Open
        long trackId = 37384;
        var streamUrl = await plexClient.GetAudioStreamUrlAsync(trackId);

        var audioClient = await voiceChannel.ConnectAsync();
        logger.LogInformation("Start Playing Track: {trackId}", trackId);
        try
        {
            await audioStreamer.PlayTrackFfmpegAsync(audioClient, streamUrl);
        }
        catch
        {
            try
            {
                await audioStreamer.PlayTrackAsync(audioClient, streamUrl);
            } catch
            {
                throw;
            }
        }
        logger.LogInformation("Done Playing Track: {trackId}", trackId);
    }
}