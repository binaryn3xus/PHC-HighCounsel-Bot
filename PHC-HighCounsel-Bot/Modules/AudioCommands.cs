using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Players;
namespace PHC_HighCounsel_Bot.Modules;

public class AudioCommands(ILogger<AudioCommands> logger, PlexClient plexClient, IAudioService audioService) : ModuleBase
{
#if DEBUG
    [SlashCommand("dev-join", "Start Plex Radio!", false, RunMode.Async)]
#else
    [SlashCommand("plex", "Start Plex Radio!", false, RunMode.Async)]
#endif
    public async Task PlexStart()
    {
        await DeferAsync();
        var user = Context.User as IGuildUser;
        var voiceChannel = user?.VoiceChannel as SocketVoiceChannel;

        var voiceState = Context.User as IVoiceState;
        if (voiceState?.VoiceChannel == null)
        {
            await ReplyAsync("You must be connected to a voice channel!");
            return;
        }

        try
        {
            // Ensure the bot joins the voice channel and starts playing a track
            long trackId = 37384; // Example track ID
            var streamUrl = await plexClient.GetAudioStreamUrlAsync(trackId);
            var filePath = await plexClient.DownloadAndSaveAsMp3Async(streamUrl);

            var retrieveOptions = new PlayerRetrieveOptions(ChannelBehavior: PlayerChannelBehavior.Join);

            var audioPlayer = await audioService.Players.RetrieveAsync(Context, PlayerFactory.Queued, retrieveOptions).ConfigureAwait(false);

            if (!audioPlayer.IsSuccess)
            {
                var errorMessage = audioPlayer.Status switch
                {
                    PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
                    PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
                    _ => "Unknown error.",
                };

                await FollowupAsync(errorMessage).ConfigureAwait(false);
            }

            var queuedPlayer = audioPlayer.Player;

            var plexList = plexClient.get

            //// Load the track
            //var loadResult = await audioService.Tracks.LoadTrackAsync("taylor", new Lavalink4NET.Rest.Entities.Tracks.TrackLoadOptions()).ConfigureAwait(false);

            //if (!loadResult.IsSuccess)
            //{
            //    await FollowupAsync("Failed to load track.").ConfigureAwait(false);
            //    return;
            //}

            //// Play the track
            //var track = loadResult.Tracks.FirstOrDefault();
            //if (track != null)
            //{
            //    await player.PlayAsync(track).ConfigureAwait(false);
            //    await FollowupAsync($"Started playing track {trackId} in {voiceChannel.Name}!").ConfigureAwait(false);
            //}
            //else
            //{
            //    await FollowupAsync("No track found.").ConfigureAwait(false);
            //}

            //// Save the text channel ID for notifications
            //_audioService.TextChannels[Context.Guild.Id] = Context.Channel.Id;
        }
        catch (Exception exception)
        {
            await RespondWithError(exception.ToString());
        }
    }

    private async Task RespondWithError(string message)
    {
        var embed = new EmbedBuilder()
            .WithTitle("Error!")
            .WithDescription(message)
            .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())
            .WithColor(Color.Red)
            .Build();

        await FollowupAsync(embed: embed);
    }
}