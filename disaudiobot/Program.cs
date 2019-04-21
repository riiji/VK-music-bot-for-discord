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
namespace disaudiobot
{


    class Program
    {
        string tokenbot = "";
        string login = "";
        string password = "";

        DiscordSocketClient _client;
        CommandHandler _handler;
        public static VkApi _vkapi;
        public static StorageData _data = new StorageData();

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose

            });
            await _client.LoginAsync(TokenType.Bot, tokenbot);
            await _client.StartAsync();

            _client.Log += Log;
            _handler = new CommandHandler();
            await VKMusic.Join(login, password);


            await _handler.InitializeAsync(_client);


            await Task.Delay(-1);
        }



        private async Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            await Task.CompletedTask;
        }




    }
}
