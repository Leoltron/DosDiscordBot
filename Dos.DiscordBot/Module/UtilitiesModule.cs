using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Dos.Utils;
using ImageMagick;

namespace Dos.DiscordBot.Module
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class UtilitiesModule : ModuleBase<SocketCommandContext>
    {
        private static readonly string[] Pongs = {"Pong!", "pong", "Ping! I mean, pong!", "...", "Yeah, I'm alive"};

        [Command("ping")]
        public async Task Ping() => await Context.Channel.SendMessageAsync(Pongs.RandomElement());

        private const string HelpMessage =
            "**dos help**             - print this message\n" +
            "**dos ping**             - Pong!\n" +
            "\n" +
            "**dos join**             - create a game or join existing\n" +
            "**dos quit**             - quit a game\n" +
            "**dos start**            - start a game\n" +
            "\n" +
            "**dos draw**             - draw card\n" +
            "**dos select <card>**    - select card to match with\n" +
            "**dos match <card(s)>**  - match cards with selected one\n" +
            "**dos add card**         - add a card to the center row\n" +
            "**dos done**             - finish your turn. You can also use this to explicitly finish matching and refill Center Row before adding cards to it. \n" +
            "\n" +
            "**dos!**                 - you have to call dos every time you got 2 cards during your turn, or else\n" +
            "**dos callout**          - you can be called out by somebody\n" +
            "\n" +
            "**dos hand**             - I will send you your hand via DM\n" +
            "**dos table**            - to see table of the game (players card count, etc.)\n" +
            "";

        [Command("help")]
        public Task Help() => Context.Channel.SendMessageAsync(HelpMessage);

        public async Task Card()
        {
            var embed = new EmbedBuilder
                {
                    Title = "Hello world!",
                    Description = "I am a description set by initializer."
                }.AddField("Field title",
                           "Field value. I also support [hyperlink markdown](https://example.com)!")
                 .WithAuthor(Context.Client.CurrentUser)
                 .WithFooter(footer => footer.Text = "I am a footer.")
                 .WithColor(Color.Blue)
                 .WithTitle("I overwrote \"Hello world!\"")
                 .WithDescription("I am a description.")
                 .WithUrl("https://example.com")
                 .WithCurrentTimestamp();

            using (var image = new MagickImage("Resources/DOS_Card_Back.png"))
            {
                using (var s = new MemoryStream(1 << 20))
                {
                    image.Write(s, MagickFormat.Png);
                    s.Seek(0, SeekOrigin.Begin);
                    await Context.Channel.SendFileAsync(s, "DOS_Card_Back.png", embed: embed.Build());
                }
            }
        }
    }
}
