using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.API;
using System.Collections.Generic;
using Discord.Audio;
using disaudiobot.Modules;
using VkNet;
using VkNet.Utils;
using VkNet.Model.Attachments;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Json;

namespace disaudiobot
{


    class Program
    {
        DiscordSocketClient _client;
        CommandHandler _handler;
        public static VkApi _vkApi;
        public static StorageData _data = new StorageData();

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose

            });

            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Config));
            using (FileStream fs = new FileStream("config.json", FileMode.OpenOrCreate))
            {
                Utils._cfg = (Config)jsonSerializer.ReadObject(fs);
            }

            Utils._cfg.Color = new Color(Utils._cfg.ColorValue);

            await _client.LoginAsync(TokenType.Bot, Utils._cfg.Token);
            await _client.StartAsync();

            _client.Log += Log;
            _handler = new CommandHandler();

            await VKMusic.AuthAsync(Utils._cfg.Login, Utils._cfg.Password);
            await _handler.InitializeAsync(_client,Utils._cfg);

            await Task.Delay(-1);
        }



        private async Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            await Task.CompletedTask;
        }




    }
}
