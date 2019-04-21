using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Model;
using VkNet.Model.Attachments;

namespace disaudiobot.Modules
{
    public class RiijiiAudio : ModuleBase<ICommandContext>
    {
        int countstars = 8; //count of stars in progress bar
        Color color = new Color(85, 172, 238); //color of embeded text
        int playlistcounter = 20;

        BinaryFormatter formatter = new BinaryFormatter();
        [Command("uploadplaylist", RunMode = RunMode.Async)]
        public async Task UploadSongs(int ownerid)
        {
            Directory.CreateDirectory($@"{Directory.GetCurrentDirectory()}\servers\{Context.Guild.Id}");

            await VKMusic.GetSongs(Program._vkapi, ownerid, Context.Guild.Id); 
            Audio[] audios;
            using (FileStream fs = new FileStream($@"{Directory.GetCurrentDirectory()}\servers\{Context.Guild.Id}\{ownerid}.dat", FileMode.OpenOrCreate, FileAccess.Read))
            {
                try
                {
                    audios = Program._data.RestoreObject<Audio[]>($"{Context.Guild.Id}_{ownerid}.dat");
                }
                catch (ArgumentException)
                {
                    Program._data.StoreObject($"{Context.Guild.Id}_{ownerid}.dat", (Audio[])formatter.Deserialize(fs));
                }
            }
            await Context.Channel.SendMessageAsync("Playlist downloaded successful");
            await Task.CompletedTask;

        }

        [Command("uploadplaylistsilent", RunMode = RunMode.Async)]
        public async Task UploadSongsSilent(int ownerid)
        {
            Directory.CreateDirectory($@"{Directory.GetCurrentDirectory()}\servers\{Context.Guild.Id}");

            Audio[] audios;


            if (!File.Exists($@"{Directory.GetCurrentDirectory()}\servers\{Context.Guild.Id}\{ownerid}.dat"))
            {
                await Context.Channel.SendMessageAsync("Trying to upload playlist...");
                await UploadSongs(ownerid);
                return;
            }

            using (FileStream fs = new FileStream($@"{Directory.GetCurrentDirectory()}\servers\{Context.Guild.Id}\{ownerid}.dat", FileMode.OpenOrCreate, FileAccess.Read))
            {
                if (fs.Length == 0)
                {
                    await Context.Channel.SendMessageAsync("Trying to serialize file...");
                    await UploadSongs(ownerid);
                    return;
                }

                try
                {
                    audios = Program._data.RestoreObject<Audio[]>($"{Context.Guild.Id}_{ownerid}.dat");
                }
                catch (ArgumentException)
                {
                    Program._data.StoreObject($"{Context.Guild.Id}_{ownerid}.dat", (Audio[])formatter.Deserialize(fs));
                }
            }
            await Task.CompletedTask;

        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayMusic(int ownerid, int startindex = 0, bool forceupdate = false)
        {
            Audio[] audio = null;

            if (forceupdate)
            {
                await Context.Channel.SendMessageAsync("Trying to force update playlist...");
                await UploadSongs(ownerid);
            }
            else
                await UploadSongsSilent(ownerid);

            audio = Program._data.RestoreObject<Audio[]>($"{Context.Guild.Id}_{ownerid}.dat");

            var channelid = (Context.User as IGuildUser)?.VoiceChannel.Id;
            var path = $@"{Directory.GetCurrentDirectory()}\servers\{Context.Guild.Id}\music.mp3";

            await StopMusic(null, false);
            await JoinChannel();

            var msg = Context.Channel.SendMessageAsync("", false, new EmbedBuilder().Build());
            IAudioClient client = Program._data.RestoreObject<IAudioClient>($"{channelid}");
            Program._data.StoreObject($"{Context.Guild.Id}.aos", client.CreatePCMStream(AudioApplication.Music));
            for (int i = startindex; i < audio.Length; ++i)
            {
                if(audio[i].ContentRestricted!=null)
                {
                    await Context.Channel.SendMessageAsync($"{audio[i].Title} is restricted");
                    continue;
                }

                CancellationTokenSource tokenSource = new CancellationTokenSource();
                Program._data.StoreObject($"{Context.Guild.Id}.cts", tokenSource);
                VKMusic.DownloadSongs(audio[i], path).Wait();
                try
                {
                    var vkuser = Program._vkapi.Users.Get(new long[] { ownerid }).FirstOrDefault();

                    Task sending = SendAsync(path);
                    Task counter =  SongCounter(msg.Result, audio[i], vkuser, tokenSource.Token);
                    await Task.WhenAny(new Task[] { sending,counter});
                    if (tokenSource.IsCancellationRequested)
                        break;
                    tokenSource.Cancel();
                }
                catch (Exception e)
                {
                    await Context.Channel.SendMessageAsync(e.Message);
                }
            }

        }

        public async Task SongCounter(IUserMessage msg, Audio audio, User vkuser, CancellationToken token)
        {
            Stopwatch clock = new Stopwatch();
            clock.Start();

            int seconds = audio.Duration / countstars;

            string msgtoserver = "";
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(color);
            embed.WithAuthor($"{vkuser.FirstName} {vkuser.LastName}");
            while (token.IsCancellationRequested == false)
            {
                msgtoserver = "";

                msgtoserver += $"**Current song:** {audio.Title}\n**Author:** {audio.Artist}\n**Duration:** {audio.Duration}s\n";

                int stars = (int)(clock.ElapsedMilliseconds / (seconds * 1000));

                for (int i = 0; i < stars; ++i)
                    msgtoserver += "🔹";

                for (int i = 0; i < countstars - stars; ++i)
                    msgtoserver += "🔸";

                embed.WithDescription(msgtoserver);
                if (msg.Embeds.FirstOrDefault().ToEmbedBuilder().Description != embed.Description)
                    await msg.ModifyAsync(x => x.Embed = embed.Build());
                Thread.Sleep(2000);
            }

            clock.Stop();
        }

        [Command("getplaylist", RunMode = RunMode.Async)]
        public async Task GetPlaylist(int ownerid, int numberlist = 0)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(color);
            await UploadSongsSilent(ownerid);
            Audio[] audio = null;
            try
            {
                audio = Program._data.RestoreObject<Audio[]>($"{Context.Guild.Id}_{ownerid}.dat");
            }
            catch (ArgumentException)
            {
                await Context.Channel.SendMessageAsync("Use !uploadplaylist [vk_id] to download a playlist");
                return;
            }

            string msg = "";
            for (int i = numberlist * playlistcounter; i <= playlistcounter * (numberlist + 1) && i < audio.Length; ++i)
            {
                msg += $"{i} | {audio[i].Artist} - {audio[i].Title}\n";
            }
            embed.WithDescription(msg);

            await Context.Channel.SendMessageAsync("",false,embed.Build());

        }

        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopMusic(IVoiceChannel channel = null, bool canceltoken = true)
        {
            if (canceltoken == true)
            {
                CancellationTokenSource token = Program._data.RestoreObject<CancellationTokenSource>($"{Context.Guild.Id}.cts");
                token.Cancel();
            }

            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            await channel.DisconnectAsync();
        }

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            // Get the audio channel
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            var audioClient = await channel.ConnectAsync();

            Console.WriteLine(new LogMessage(LogSeverity.Verbose, "BOT", $"joined to {channel.Id}"));

            Program._data.StoreObject($"{channel.Id}", audioClient);

            await Task.CompletedTask;
        }



        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 -b 96 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,

            });
        }

        private async Task SendAsync(string path)
        {
            AudioOutStream discord = Program._data.RestoreObject<AudioOutStream>($"{Context.Guild.Id}.aos");

            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            {
                try
                {
                    await output.CopyToAsync(discord);

                }
                finally
                {
                    await discord.FlushAsync();
                    
                }

            }
            await Task.CompletedTask;
        }
    }
}
