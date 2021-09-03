using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using Interactivity.Selection;
using Victoria;
using Victoria.Enums;
using Interactivity;
using Victoria.Payloads;
using Interactivity.Pagination;
using Victoria.Responses.Search;
using Victoria.Filters;

namespace AwesomeBot.Modules
{
    [Summary("Does lots of different things like play music!")]
    public class Media : ModuleBase
    {
        public static LavaNode _lavaNode;
        DiscordSocketClient _discord;
        public InteractivityService Interactivity { get; set; }
        public static bool isLooping { get; set; }
        public Media(LavaNode lavanode, DiscordSocketClient discord)
        {
            _lavaNode = lavanode;

            _discord = discord;

        }

        [Command("loop")]
        public async Task LoopAsync()
        {
            if (!isLooping)
            {
                isLooping = true;
                await ReplyAsync("Started Looping");
                return;
            }
            isLooping = false;
            await ReplyAsync("Stopped Looping");
            return;
        }

        [Command("Play", RunMode = RunMode.Async)]
        [Alias("p")]
        [Summary("search Youtube for a song and play it in a VC!")]
        
        public async Task PlayAsync([Remainder] string searchQuery )
        {

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
                await ReplyAsync("You're not in a voice channel! Please join one to use this command.");
                return;
            }
            
            
            var searchResponse = await _lavaNode.SearchYouTubeAsync(searchQuery);
            if (searchResponse.Status == SearchStatus.LoadFailed ||
                searchResponse.Status == SearchStatus.NoMatches)
            {
                await ReplyAsync($"I wasn't able to find anything for `{searchQuery}`.");
                return;
            }

            List<PageBuilder> pages = new List<PageBuilder>();
            int maxResults = 5;
            int pageCount = searchResponse.Tracks.Count / maxResults;
            try
            {
                for (int i = 0; i < pageCount; i++)
                {
                    var builder = new PageBuilder()
                        .WithTitle($"Now Showing: Page {i + 1}")
                        .WithDescription($"Results for {searchQuery}, page {i + 1} of {pageCount}");
                    for (int j = 0; j < maxResults; j++)
                    {
                        int number = j + (i * 5);
                            var _track = searchResponse.Tracks.ToList()[number];
                            builder.AddField($"[{number + 1}] _{ _track.Title}_", _track.Url, false);
                    }
                    pages.Add(builder);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("" + e);
            }

            var paginator = new StaticPaginatorBuilder()
                .WithUsers(Context.User as SocketUser)
                .WithPages(pages.ToArray())
                .WithFooter(PaginatorFooter.PageNumber | PaginatorFooter.Users)
                .WithDefaultEmotes()
                .Build();
            
            //basically dont await this or you cant reply until
            //the paginated message times out which is never
            
            Interactivity.SendPaginatorAsync(paginator, Context.Channel);
            var response = await Interactivity.NextMessageAsync(x => x.Author == Context.User);
            int song = 0;
            try
            {
                song = int.Parse(response.Value.Content) - 1;
            }
            catch
            {
                await ReplyAsync("Please enter a number!");
                return;
            }
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }
            var track = searchResponse.Tracks.ToList()[song];
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
                if (!isLooping)
                {
                    
                    await player.SkipAsync();
                }
                else
                {
                    await ReplyAsync("Can't skip while looping!");
                }
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
            if (player.Queue.Count > 0)
            {
                List<PageBuilder> pages = new List<PageBuilder>();
                int maxResults = 5;
                
                int pageCount = player.Queue.Count / maxResults;
                try
                {
                    for (int i = 0; i < pageCount; i++)
                    {
                        var builder = new PageBuilder()
                            .WithTitle($"Now Showing: Page {i + 1}")
                            .WithDescription($"Queue, page {i + 1} of {pageCount}");
                        for (int j = 0; j < maxResults; j++)
                        {
                            int number = j + (i * 5);
                            var _track = player.Queue.ToList()[number];
                            builder.AddField($"[{number + 1}] _{ _track.Title}_", _track.Url, false);
                        }
                        pages.Add(builder);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("" + e);
                }

                var paginator = new StaticPaginatorBuilder()
                    .WithUsers(Context.User as SocketUser)
                    .WithPages(pages.ToArray())
                    .WithFooter(PaginatorFooter.PageNumber | PaginatorFooter.Users)
                    .WithDefaultEmotes()
                    .Build();

                //basically dont await this or you cant reply until
                //the paginated message times out which is never

                Interactivity.SendPaginatorAsync(paginator, Context.Channel);
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

        [Command("Equalizer")]
        [Summary("Set an Equalizer for the player")]
        [Alias("eq")]
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

                await ReplyAsync($"Applied gain {_gain} to band {String.Join(", ", EQBands)}");
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
