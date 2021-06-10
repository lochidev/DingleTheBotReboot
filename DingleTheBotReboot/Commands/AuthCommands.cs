using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace DingleTheBotReboot.Commands
{
    public class AuthCommands : BaseCommandModule
    {
        [Command("sendVerifyMessage")]
        [RequireOwner]
        public async Task SendVerifyMessage(CommandContext ctx)
        {
            DiscordMessageBuilder builder = new DiscordMessageBuilder();
            DiscordButtonComponent verifyBtn = new DiscordButtonComponent(ButtonStyle.Primary, "verifyBtn", "Let me in already! :(", false, new DiscordComponentEmoji(333698513604968450));

            builder.WithContent("Hold your horses! Are you not a bot? Prove it!").AddComponents(verifyBtn);

            await builder.SendAsync(ctx.Channel);
        }
    }
}
