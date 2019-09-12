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
    public class RiijiiAudio : ModuleBase<SocketCommandContext>
    {
        Color color = new Color(85, 172, 238);
        BinaryFormatter formatter = new BinaryFormatter();


        /// <summary>
        /// Uploading playlist in memory
        /// </summary>
        /// <param name="ownerid"></param>
        /// <returns></returns>
        [Command("uploadplaylist", RunMode = RunMode.Async)]
        public async Task UploadPlaylist(int ownerid)
        {
            Directory.CreateDirectory($@"{Directory.GetCurrentDirectory()}\servers\{Context.Guild.Id}");

            await VKMusic.GetPlaylistInFile(Program._vkApi, ownerid, Context.Guild.Id);

            using (FileStream fs = new FileStream($@"{Directory.GetCurrentDirectory()}\servers\{Context.Guild.Id}\{ownerid}.dat", FileMode.OpenOrCreate, FileAccess.Read))
            {
                Program._data.StoreObject($"{Context.Guild.Id}_{ownerid}.dat", (Audio[])formatter.Deserialize(fs));
            }

            await Context.Channel.SendMessageAsync("Playlist downloaded successful");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Uploading playlist with file
        /// </summary>
        /// <param name="ownerid">VK user id</param>
        /// <returns></returns>
        [Command("uploadplaylistsilent", RunMode = RunMode.Async)]
        public async Task UploadPlaylistSilent(int ownerid)
        {
            Directory.CreateDirectory($@"{Directory.GetCurrentDirectory()}\servers\{Context.Guild.Id}");

            if (!File.Exists($@"{Directory.GetCurrentDirectory()}\servers\{Context.Guild.Id}\{ownerid}.dat"))
            {
                await Context.Channel.SendMessageAsync("Trying to upload playlist...");
                await UploadPlaylist(ownerid);
                return;
            }

            using (FileStream fs = new FileStream($@"{Directory.GetCurrentDirectory()}\servers\{Context.Guild.Id}\{ownerid}.dat", FileMode.OpenOrCreate, FileAccess.Read))
            {
                if (fs.Length == 0)
                {
                    await Context.Channel.SendMessageAsync("Trying to serialize file...");
                    await UploadPlaylist(ownerid);
                    return;
                }

                Program._data.StoreObject($"{Context.Guild.Id}_{ownerid}.dat", (Audio[])formatter.Deserialize(fs));
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
                await UploadPlaylist(ownerid);
            }
            else
                await UploadPlaylistSilent(ownerid);

            audio = Program._data.RestoreObject<Audio[]>($"{Context.Guild.Id}_{ownerid}.dat");

            var channelId = (Context.User as IGuildUser)?.VoiceChannel.Id;
            var path = $@"{Directory.GetCurrentDirectory()}\servers\{Context.Guild.Id}\music.mp3";

            await StopMusic(null, false);
            await JoinChannel();

            var msg = Context.Channel.SendMessageAsync("", false, new EmbedBuilder().Build());
            IAudioClient client = Program._data.RestoreObject<IAudioClient>($"{channelId}");
            Program._data.StoreObject($"{Context.Guild.Id}.aos", client.CreatePCMStream(AudioApplication.Music));
            for (int i = startindex; i < audio.Length; ++i)
            {
                if (audio[i].ContentRestricted != null)
                {
                    await Context.Channel.SendMessageAsync($"{audio[i].Title} is restricted");
                    continue;
                }

                CancellationTokenSource tokenSource = new CancellationTokenSource();
                Program._data.StoreObject($"{Context.Guild.Id}.cts", tokenSource);
                VKMusic.DownloadSongs(audio[i], path).Wait();
                try
                {
                    var vkuser = Program._vkApi.Users.Get(new long[] { ownerid }, VkNet.Enums.Filters.ProfileFields.Photo200).FirstOrDefault();

                    Task counter = SongCounter(msg.Result, audio[i], vkuser, i, tokenSource.Token);
                    Task sending = SendAsync(path);
                    await Task.WhenAny(new Task[] { sending, counter });
                    if (tokenSource.IsCancellationRequested)
                        break;
                    tokenSource.Cancel();
                    Console.WriteLine(new LogMessage(LogSeverity.Info, "BOT", "Token cancelled"));
                }
                catch (Exception e)
                {
                    await Context.Channel.SendMessageAsync(e.Message);
                }
            }

        }

        public async Task SongCounter(IUserMessage msg, Audio audio, User vkuser, int songnumber, CancellationToken token)
        {
            Stopwatch clock = new Stopwatch();
            clock.Start();

            int seconds = audio.Duration / Utils._cfg.StarsCount;

            string msgToServer = "";
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(color);
            embed.WithAuthor($"{vkuser.FirstName} {vkuser.LastName}", vkuser.Photo200.AbsoluteUri, ("https://vk.com/id" + vkuser.Id));

            while (token.IsCancellationRequested == false)
            {
                msgToServer = "";

                msgToServer += $"**Current song:** {audio.Title}\n**Author:** {audio.Artist}\n**Duration:** {audio.Duration}s\n**Number:** {songnumber}\n";

                int stars = (int)(clock.ElapsedMilliseconds / (seconds * 1000));

                for (int i = 0; i < stars; ++i)
                    msgToServer += "🔹";

                for (int i = 0; i < Utils._cfg.StarsCount - stars; ++i)
                    msgToServer += "🔸";

                embed.WithDescription(msgToServer);
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
            await UploadPlaylistSilent(ownerid);
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
            for (int i = numberlist * Utils._cfg.GetPlaylistCount; i <= Utils._cfg.GetPlaylistCount * (numberlist + 1) && i < audio.Length; ++i)
            {
                msg += $"{i} | {audio[i].Artist} - {audio[i].Title}\n";
            }
            embed.WithDescription(msg);

            await Context.Channel.SendMessageAsync("", false, embed.Build());

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
