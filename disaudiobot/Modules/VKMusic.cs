using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VkNet;
using VkNet.AudioBypassService;
using VkNet.AudioBypassService.Extensions;
using VkNet.Utils;
using VkNet.Model.Attachments;
using System.IO;
using Discord.Commands;
using System.Runtime.Serialization.Formatters.Binary;
using Discord;
using System.Threading;

namespace disaudiobot.Modules
{
    class VKMusic
    {
        /// <summary>
        /// Auth in VK async
        /// </summary>
        /// <param name="login">VK login</param>
        /// <param name="password">VK password</param>
        /// <returns></returns>
        public static async Task AuthAsync(string login, string password)
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();
            var api = new VkApi(services);
            await api.AuthorizeAsync(new VkNet.Model.ApiAuthParams
            {
                Login = login,
                Password = password
            });
            Program._vkApi = api;
            Console.WriteLine(new LogMessage(LogSeverity.Verbose,"VK.net","Joined"));
        }

        /// <summary>
        /// Auth in VK
        /// </summary>
        /// <param name="login">VK login</param>
        /// <param name="password">VK password</param>
        public static void Auth(string login, string password)
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();
            var api = new VkApi(services);
            api.Authorize(new VkNet.Model.ApiAuthParams
            {
                Login = login,
                Password = password
            });
            Program._vkApi = api;
            Console.WriteLine(new LogMessage(LogSeverity.Verbose, "VK.net", "Joined"));
        }

        /// <summary>
        /// Get a VK user playlist into \servers\*current server*\*userid*.dat
        /// </summary>
        /// <param name="api">VK account</param>
        /// <param name="ownerid">VK user's id</param>
        /// <param name="guildid">User's guildid</param>
        /// <returns></returns>
        public static async Task GetPlaylistInFile(VkApi api, int ownerid, ulong guildid)
        {
            VkCollection<Audio> audios = null;
            try
            {
                audios = await api.Audio.GetAsync(new VkNet.Model.RequestParams.AudioGetParams { OwnerId = ownerid });
            }
            catch (Exception)
            {
                Console.WriteLine(new LogMessage(LogSeverity.Error,"Vk.net","Cant get audio(Token confirmation)"));

                return;
            }

            Audio[] audio = audios.ToArray();

            BinaryFormatter formatter = new BinaryFormatter();

            string uri = $@"{Directory.GetCurrentDirectory()}\servers\{guildid}\{ownerid}.dat";

            using (FileStream fs = new FileStream(uri, FileMode.OpenOrCreate, FileAccess.Write))
            {
                formatter.Serialize(fs, audio);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Download song into \servers
        /// </summary>
        /// <param name="Song">Song</param>
        /// <param name="name">Name of file(music.mp3)</param>
        /// <returns></returns>
        public static async Task DownloadSongs(Audio Song, string name)
        {

            if (Song.Url == null || name == null)
            {
                if (Song.Url == null)
                    throw new ArgumentException("Song url wasn't found");
                if (name == null)
                    throw new ArgumentException("Name is equal to null!");
            }

            // trying to fix corrupted url
            if(Song.Url.AbsoluteUri.Contains("m3u8"))
            {
                Song.Url = Utils.FixUrl(Song.Url);
            }

            Console.WriteLine(new LogMessage(LogSeverity.Info, "BOT", $"{Song.Url}"));

            
            // downloading music file from vk server
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(Song.Url, name);
                Console.WriteLine(new LogMessage(LogSeverity.Verbose, "BOT", "Sound downloaded"));
                Thread.Sleep(100);
                await Task.CompletedTask;
            }
        }
    }
}
