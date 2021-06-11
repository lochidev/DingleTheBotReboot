using DSharpPlus.SlashCommands;
using System.Linq;
using System.Threading.Tasks;

namespace DingleTheBotReboot.Attributes
{
    public class RequireRolesAttribute : SlashCheckBaseAttribute
    {
        private readonly string[] Roles;
        public RequireRolesAttribute(params string[] roles)
        {
            Roles = roles;
        }
        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            return Task.FromResult(ctx.Member.Roles.Any(x => Roles.Any(y => x.Name == y)));
        }
    }
}
