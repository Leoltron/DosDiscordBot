using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord;
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
            "**dos invite**            - invite this bot to your server\n" +
            "**dos support**           - get link to a server where you ask about this bot, report bugs, etc.\n" +
            "\n" +
            "**dos join**              - create a game or join existing\n" +
            "**dos add-bot**           - add AI player\n" +
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
        [Alias("server")]
        public Task Support() => SendToUserAndNotifyChannel(DosBot.SupportLink, "DM'd you link to a support server");

        [Command("invite")]
        public Task Invite() => SendToUserAndNotifyChannel(DosBot.InviteLink, "DM'd you my invite link");


        [Command("source")]
        [Alias("github", "repo", "git")]
        public Task Source() => SendToUserAndNotifyChannel(DosBot.RepoLink, "DM'd you my source link");

        private async Task SendToUserAndNotifyChannel(string message, string channelMessage)
        {
            var channelIsPrivate = Context.Channel is IPrivateChannel;
            try
            {
                await Context.User.SendMessageAsync(message);
            }
            catch (Exception)
            {
                if (!channelIsPrivate)
                    await Context.Channel.SendMessageAsync(
                        "Failed to DM you. Please check if your DM is open and try again.");
                return;
            }

            if (!channelIsPrivate)
                await Context.Channel.SendMessageAsync(channelMessage);
        }
    }
}
