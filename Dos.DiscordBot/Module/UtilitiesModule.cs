using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord.Commands;
using Dos.Utils;

namespace Dos.DiscordBot.Module
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class UtilitiesModule : ModuleBase<SocketCommandContext>
    {
        private const string HelpMessage =
            "**dos help**              - print this message\n" +
            "**dos ping**              - Pong!\n" +
            "\n" +
            "**dos join**              - create a game or join existing\n" +
            "**dos quit**              - quit a game\n" +
            "**dos start**             - start a game\n" +
            "**dos config**            - get configuration of the current game\n" +
            "**dos config <key>**      - get configuration description\n" +
            "**dos config <key> <val>**- set configuration value\n" +
            "\n" +
            "**dos draw**              - draw card\n" +
            "**dos select <card>**     - select card from Center Row to match with\n" +
            "**dos match <card(s)>**   - match cards with selected one\n" +
            "**dos add card**          - add a card to the Center Row\n" +
            "**dos done**              - finish your turn. You can also use this to explicitly finish matching and refill Center Row before adding cards to it. \n" +
            "\n" +
            "**dos!**                  - you have to call dos every time you got 2 cards during your turn, or else\n" +
            "**dos callout**           - you can be called out by somebody\n" +
            "\n" +
            "**dos hand**              - I will send you your hand via DM\n" +
            "**dos table**             - to see table of the game (players card count, etc.)\n" +
            "\n" +
            "Combine commands with `dos <command1> && <command2>`. For example, `dos add g1 && !`" +
            "";

        private static readonly string[] Pongs = {"Pong!", "pong", "Ping! I mean, pong!", "...", "Yeah, I'm alive"};

        [Command("ping")]
        public async Task Ping() => await Context.Channel.SendMessageAsync(Pongs.RandomElement());

        [Command("help")]
        public Task Help() => Context.Channel.SendMessageAsync(HelpMessage);

        private const string ComingSoon = "I'm not ready for the big world yet, but I will soon...";

        [Command("support")]
        public Task Support() => Context.Channel.SendMessageAsync(ComingSoon);

        [Command("invite")]
        public Task Invite() => Context.Channel.SendMessageAsync(ComingSoon);

        [Command("source")]
        [Alias("github")]
        public Task Source() => Context.Channel.SendMessageAsync(ComingSoon);
    }
}
