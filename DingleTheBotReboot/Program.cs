using DingleTheBotReboot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.Threading.Tasks;

namespace DingleTheBotReboot
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var discord = new DiscordShardedClient(new DiscordConfiguration
            {
                Token = Environment.GetEnvironmentVariable("botToken"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });

            var commands = await discord.UseCommandsNextAsync(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" }
            });
            foreach (var command in commands.Values)
            {
                command.RegisterCommands<BasicCommands>();
            }
            await discord.StartAsync();
            await Task.Delay(-1);
        }
    }
}
