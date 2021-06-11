using DingleTheBotReboot.Attributes;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace DingleTheBotReboot.Commands
{
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
        [SlashCommand("mute", "Mute the offender :o")]
        public async Task Mute(InteractionContext ctx, [Option("user", "The user to mute")] DiscordUser user, [Option("reason", "Reason for mute")] string reason = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (user is null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("User cannot be null!"));
            }
            await ((DiscordMember)user).GrantRoleAsync(ctx.Guild?.GetRole(753129139372032111), reason);
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = $"Muted User {user.Username}",
            }
            .WithFooter($"Requested by {ctx.Member.DisplayName}", ctx.Member.AvatarUrl)
            .WithAuthor($"{user.Username}", user.AvatarUrl, user.AvatarUrl)
            .WithDescription(reason is not null ? $"With reason: {reason}" : "No reason provided");
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
        }
        [SlashCommand("ban", "Totally Real Ban")]
        [RequireRoles("RADmin")]
        public async Task Ban(InteractionContext ctx, [Option("user", "The user to ban ;)")] DiscordUser user = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (user is null)
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = $"Ban who",
                    ImageUrl = "https://i.pinimg.com/originals/04/cc/38/04cc3802ec5fd9b3655f47e488be3a91.gif"
                }
                .WithFooter($"Requested by {ctx.Member.DisplayName}", ctx.Member.AvatarUrl)
                .WithAuthor($"{user.Username}", user.AvatarUrl, user.AvatarUrl);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
            }
            else
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = $"Banned user {user.Username}",
                    ImageUrl = "https://i.pinimg.com/originals/75/cd/f6/75cdf69a0efeaa22fe85fe66254e4330.gif"
                }
                .WithFooter($"Requested by {ctx.Member.DisplayName}", ctx.Member.AvatarUrl)
                .WithAuthor($"{user.Username}", user.AvatarUrl, user.AvatarUrl);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
            }
        }
    }
}
