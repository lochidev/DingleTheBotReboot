﻿using DingleTheBotReboot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DingleTheBotReboot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            var dI = new ServiceCollection().AddSingleton<Random>().BuildServiceProvider();
            DiscordShardedClient discord = new DiscordShardedClient(new DiscordConfiguration
            {
                Token = Environment.GetEnvironmentVariable("BOT_TOKEN"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                MinimumLogLevel = LogLevel.Debug
            });
            System.Collections.Generic.IReadOnlyDictionary<int, CommandsNextExtension> commands = await discord.UseCommandsNextAsync(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "dalpha" },
                Services = dI
            });
            foreach (CommandsNextExtension command in commands.Values)
            {
                command.RegisterCommands<AuthCommands>();
            }
            System.Collections.Generic.IReadOnlyDictionary<int, SlashCommandsExtension> slashCommands = await discord.UseSlashCommandsAsync(new SlashCommandsConfiguration()
            {
                Services = dI
            });
            foreach (SlashCommandsExtension command in slashCommands.Values)
            {
                command.RegisterCommands<BasicSlashCommands>(738657617248911472);
            }
            await discord.StartAsync();
            StartupEvents.Initialize(discord);
            await Task.Delay(-1);
        }
    }
}
