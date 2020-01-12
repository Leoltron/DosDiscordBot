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


        [Command("card")]
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
                using (var s = new MemoryStream(1<<20))
                {
                    image.Write(s, MagickFormat.Png);
                    s.Seek(0, SeekOrigin.Begin);
                    await Context.Channel.SendFileAsync(s, "DOS_Card_Back.png", embed: embed.Build());
                }
            }
        }
    }
}
