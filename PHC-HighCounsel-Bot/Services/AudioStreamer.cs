using Discord.Audio;
using System.Diagnostics;

namespace PHC_HighCounsel_Bot.Services;

public class AudioStreamer(PlexClient plexClient, DiscordSocketClient client)
{

    public async Task PlayTrackAsync(IAudioClient audioClient, string streamUrl, CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();
        using var audioStream = await httpClient.GetStreamAsync(streamUrl, cancellationToken);
        using var discordStream = audioClient.CreatePCMStream(AudioApplication.Mixed);
        try
        {
            await audioStream.CopyToAsync(discordStream);
        }
        finally
        {
            await discordStream.FlushAsync();
        }
    }

    public async Task PlayTrackFfmpegAsync(IAudioClient audioClient, string streamUrl, CancellationToken cancellationToken = default)
    {
        var ffmpeg = new ProcessStartInfo
        {
            FileName = "/App/ffmpeg", // Path to the ffmpeg executable in the /App directory
            Arguments = $"-i {streamUrl} -f s16le -ar 48000 -ac 2 pipe:1", // Convert to PCM format
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = "/App"
        };

        try
        {
            using var process = Process.Start(ffmpeg);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start ffmpeg process");
            }

            using var audioStream = process.StandardOutput.BaseStream;
            using var discordStream = audioClient.CreatePCMStream(AudioApplication.Music);

            await audioStream.CopyToAsync(discordStream, cancellationToken).ConfigureAwait(false);
            await discordStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting ffmpeg process: {ex.Message}");
        }
    }
}
