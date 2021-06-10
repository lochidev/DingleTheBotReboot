using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace DingleTheBotReboot.Commands
{
    [SlashCommandGroup("group", "description")]
    public class BasicSlashCommands : SlashCommandModule
    {
        [SlashCommand("ping", "check whether the bot is up :)")]
        public async Task PingCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Pong!"));
        }
        [SlashCommand("Beg", "The classic beg command!")]
        public async Task BegCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Not implemented yet!"));
        }
        [SlashCommand("avatar", "Get someone's avatar")]
        public async Task Avatar(InteractionContext ctx, [Option("user", "The user to get it for")] DiscordUser user = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            user ??= ctx.Member;
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = $"Avatar",
                ImageUrl = user.AvatarUrl
            }.
            WithFooter($"Requested by {ctx.Member.DisplayName}", ctx.Member.AvatarUrl).
            WithAuthor($"{user.Username}", user.AvatarUrl, user.AvatarUrl);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
        }
    }
}
