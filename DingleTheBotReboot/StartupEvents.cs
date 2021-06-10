using DSharpPlus;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace DingleTheBotReboot
{
    public static class StartupEvents
    {
        public static void Initialize(DiscordShardedClient discord)
        {
            discord.ComponentInteractionCreated += (s, e) =>
            {
                _ = Task.Run(async () =>
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);
                    if (e.Id == "verifyBtn")
                    {
                        await ((DiscordMember)e.User).GrantRoleAsync(e.Guild?.GetRole(787146778357137430), "Successfully verified!");
                    }
                });

                return Task.CompletedTask;
            };
        }
    }
}
