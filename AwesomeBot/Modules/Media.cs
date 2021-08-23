using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using AwesomeBot.Services;
using System.Reflection;
using Discord.Addons.Interactive;
using Victoria;
using Victoria.Enums;
using Victoria.Responses;
using Victoria.Responses.Search;
using Victoria.Filters;
using Microsoft.Extensions.Configuration;

namespace AwesomeBot.Modules
{
    [Summary("Does lots of different things like play music!")]
    public class Media: InteractiveBase<SocketCommandContext>
    {
        public static LavaNode _lavaNode;
        public static IConfigurationRoot _config;
        DiscordSocketClient _discord;
        public static bool isLooping { get; set; }
        public Media(LavaNode lavanode, IConfigurationRoot config, DiscordSocketClient discord)
        {
            _lavaNode = lavanode;
            _config = config;
            _discord = discord;

            _lavaNode.OnTrackEnded += _lavaNode_OnTrackEnded;

        }
        private async Task _lavaNode_OnTrackEnded(Victoria.EventArgs.TrackEndedEventArgs arg)
        {
            var player = arg.Player;
            if (isLooping)
            {
                await player.SeekAsync(new TimeSpan(0, 0, 0));
                return;
            }
            else
            {
                await ReplyAsync("Not looping");
                return;
            }
        }

        [Command("Play", RunMode = RunMode.Async)]
        [Alias("p")]
        [Summary("search Youtube for a song and play it in a VC!")]
        
        public async Task PlayAsync([Remainder] string searchQuery )
        {

            bool timeout = false;
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                await ReplyAsync("Please provide search terms.");
                return;
            }
            var voiceState = Context.User as IVoiceState;
            if (voiceState.VoiceChannel != null)
            {
                if (!_lavaNode.HasPlayer(Context.Guild))
                {
                    await ReplyAsync("I'm not connected to a voice channel, joining a voice channel...");
                    await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                    await ReplyAsync($"Joined **{voiceState.VoiceChannel.Name}**!");

                }
            }
            else
            {
                await ReplyAsync("You're not in a voice channel");
                return;
            }


            var searchResponse = await _lavaNode.SearchAsync(SearchType.YouTube, searchQuery);
            if (searchResponse.Status == SearchStatus.LoadFailed ||
                searchResponse.Status == SearchStatus.NoMatches)
            {
                await ReplyAsync($"I wasn't able to find anything for `{searchQuery}`.");
                return;
            }


            int i = 0;
            var builder = new EmbedBuilder();
            builder.Title = $"🎶 **_Now showing Results for {searchQuery}:_** 🎶 ";
            builder.Footer = new EmbedFooterBuilder()
                .WithText(" Reply with a number for which song you would like!");

            

            foreach (var _track in searchResponse.Tracks.ToList())
            {
                builder.AddField($"[{i + 1}] _{_track.Title}_ ♪", _track.Url, false);

                i++;
            }

            var embed = builder.Build();
            await ReplyAsync(null, false, embed);

            var response = await NextMessageAsync() as SocketUserMessage;
            int song = 0;
            if (response != null)
            {
                try
                {
                    song = int.Parse(response.Content) - 1;
                }
                catch
                {
                    await ReplyAsync("Please enter a number");
                    return;
                }
            }
            else
            {
                timeout = true;
                await ReplyAsync("Response timeout, try again");
            }
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            var track = searchResponse.Tracks.ToList()[song];
            if (!timeout)
            {
                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    player.Queue.Enqueue(track);
                    await ReplyAsync($"🎶 Queued: **_{track.Title}_**");
                }
                else
                {
                    await player.PlayAsync(track);
                    await ReplyAsync($"🎶 Now playing: **_{track.Title}_**");
                }
            }
        }
        [Command("pause")]
        [Summary("pause a currently playing song")]
        public async Task PauseAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            if (player.PlayerState == PlayerState.Paused)
            {
                await ReplyAsync("Already paused!");
            }
            else
            {
                await player.PauseAsync();
                await ReplyAsync($"🎶 Paused **_{player.Track.Title}_**");

            }
        }
        [Command("resume")]
        [Summary("resume a song if it is paused")]
        public async Task PlayAsyncAfterPause()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            if (player.PlayerState == PlayerState.Paused)
            {
                await player.ResumeAsync();
                await ReplyAsync($"🎶 Now playing:**_{player.Track.Title}_**");
            }
            else
            {
                await ReplyAsync("Already playing!");

            }
        }
        [Command("stop")]
        [Alias("st", "leave")]
        [Summary("stops playing music and leaves VC.")]
        public async Task StopAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            if (player.PlayerState == PlayerState.Stopped)
            {
                await ReplyAsync("Nothing is playing.");
                var voiceState = Context.User as IVoiceState;
                await _lavaNode.LeaveAsync(voiceState.VoiceChannel);
                await ReplyAsync($"Left **_{voiceState.VoiceChannel.Name}!_**");
            }
            else
            {
                await player.StopAsync();
                await ReplyAsync("Stopped playing.");
                var voiceState = Context.User as IVoiceState;
                await _lavaNode.LeaveAsync(voiceState.VoiceChannel);
                await ReplyAsync($"Left **_{voiceState.VoiceChannel.Name}!_**");
            }
        }
        [Command("skip")]
        [Alias("sk")]
        [Summary("skips current song.")]
        public async Task SkipAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            if (!(player.Queue.Count == 0))
            {
                await player.SkipAsync();
                await ReplyAsync($"Skipped {player.Track.Title}");
            }
            else
            {
                await ReplyAsync("Nothing in the queue, can't skip.");
            }
        }
        [Command("Queue")]
        [Alias("q")]
        [Summary("Shows the current queue for songs.")]
        public async Task showQueueAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            string stringBuilder = "";
            int i = 0;
            if (player.Queue.Count > 0)
            {
                foreach (var track in player.Queue)
                {
                    stringBuilder += $"\n [{i + 1}] {track.Title} ";
                    i++;
                }
                var embed = new EmbedBuilder()
                    .AddField("🎶 Now showing Queued songs: 🎶 ", stringBuilder, true);
                await ReplyAsync(null, false, embed.Build());
            }
            else
            {
                await ReplyAsync("There's nothing in the queue.");
            }
        }
        [Command("restart")]
        [Alias("r")]
        [Summary("Restarts the current song.")]
        public async Task restartSongAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            await player.SeekAsync(new TimeSpan(0, 0, 0, 0, 0));
            await ReplyAsync($"🎶 Restarted **_{player.Track.Title}_**");
        }
        [Command("volume")]
        [Alias("v")]
        [Summary("Sets volume for bot")]
        public async Task SetVolumeAsync(int volume)
        {
            
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            if (volume < 200 && volume > 0)
            {
                await player.UpdateVolumeAsync(Convert.ToUInt16(volume));
                
            }
            else
            {
                if (volume >= 200)
                {
                    await ReplyAsync("Number is too high");
                }
                else if (volume <= 0)
                {
                    await ReplyAsync("Number is too low");


                }
            }

        }
        [Command("Seek")]
        [Summary("Skip to a certain timestamp on a song")]
        public async Task SeekAsync([Remainder] string time)
        {
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            try
            {
                hours = int.Parse(time.Substring(0, 2));
                minutes = int.Parse(time.Substring(3, 2));
                seconds = int.Parse(time.Substring(6, 2));
            }
            
            catch
            {
                await ReplyAsync("Time was not in correct format. Please use HH:MM:SS");
                return;
            }
            
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            TimeSpan seekPosition = new TimeSpan(hours, minutes, seconds);
            if (seekPosition > player.Track.Duration)
            {
                await ReplyAsync("Cannot seek past song duration.");
                return;
            }

            await player.SeekAsync(seekPosition);
            await ReplyAsync($"Playing from **_{hours}:{minutes}:{seconds}._**");
        }
        [Command("nowplaying")]
        [Alias("current", "playing")]
        [Summary("Get the song that is currently playing.")]
        public async Task GetCurrentSong()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            await ReplyAsync($"🎶 Now playing:**_{player.Track.Title}_**");


        }
        [Command("Loop")]
        [Summary("Sets the current song to loop")]
        public async Task loopAsync()
        {
            isLooping = true;
            await ReplyAsync("Looping");

        }
        [Command("Equalizer")]
        [Summary("")]
        public async Task EqualizeAsync(string band, [Remainder]string gain)
        {
            int _band = 0;
            int _gain = 0;
            List<EqualizerBand> EQBands = new List<EqualizerBand>();
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            if (band.Contains("-"))
            {
                string[] bands = band.Split("-");
                
                try
                {
                    _gain = int.Parse(gain);
                    int first = int.Parse(bands[0]);
                    int second = int.Parse(bands[1]);
                    int size = second - first + 1;
                    double slope = (-0.25f - 1) / (-100 - 100);
                    double output = -0.25f + slope * (_gain - -100);
                    for (int i = 0; i < size; i++)
                    {
                        EQBands.Add(new EqualizerBand(i + first ,output));
                    }
                    

                }
                catch(ArgumentException e)
                {
                    await ReplyAsync($"Error occurred: {e}");
                    return;
                }
                await player.EqualizerAsync(EQBands.ToArray());

                await ReplyAsync($"Applied gain {_gain} to band {_band}");
            }
            else
            {
                try
                {
                    _band = int.Parse(band);
                    _gain = int.Parse(gain) + 1;

                }
                catch (ArgumentException e)
                {
                    await ReplyAsync($"Error occurred: {e}");
                    return;
                }
                if (!(_gain > -101 && _gain < 101))
                {
                    await ReplyAsync("Enter a gain value from `-100 - 100`");
                    return;
                }
                if (!(_band > 0 && _band < 16))
                {
                    await ReplyAsync("Enter a band value from `0 - 15`");
                    return;
                }
                double slope = (-0.25f - 1) / (-100 - 100);
                double output = -0.25f + slope * (_gain - -100);
                await player.EqualizerAsync(new EqualizerBand(_band, output));

                await ReplyAsync($"Applied gain {_gain} to band {_band}");
            }
        }
    }
}
