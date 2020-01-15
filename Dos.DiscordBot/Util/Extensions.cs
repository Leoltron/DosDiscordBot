using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Dos.DiscordBot.Commands;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Utils;
using Serilog.Events;

namespace Dos.DiscordBot.Util
{
    public static class Extensions
    {
        public static DiscordDosGame GetGame(this ICommandContext context) => (context as DosCommandContext)?.DosGame;

        public static LogEventLevel ToLogLevel(this LogSeverity severity)
        {
            return severity switch
            {
                LogSeverity.Critical => LogEventLevel.Fatal,
                LogSeverity.Error => LogEventLevel.Error,
                LogSeverity.Warning => LogEventLevel.Warning,
                LogSeverity.Info => LogEventLevel.Information,
                LogSeverity.Verbose => LogEventLevel.Debug,
                LogSeverity.Debug => LogEventLevel.Verbose,
                _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
            };
        }

        public static Task<IUserMessage> SendCards(this IUser user, IEnumerable<Card> cards, bool images,
                                                   bool newDealtCards = false)
        {
            var cardList = cards.OrderByColorAndValue().ToList();
            return images
                ? user.SendCardsImages(cardList, newDealtCards)
                : user.SendCardsNames(cardList, newDealtCards);
        }

        public static Task<IUserMessage> SendCards(this IMessageChannel channel, IEnumerable<List<Card>> cards)
        {
            var cardList = cards.OrderBy(c => c.First().Color).ThenBy(c => c.First().Value).ToList();
            var name = string.Join("_", cardList.Select(cl => string.Join("-", cl.Select(c => c.ToShortString())))) +
                       ".png";

            return channel.SendFileAsync(cardList.Select(CardToImageHelper.Stack).JoinImages(10, 1173), name);
        }

        public static Task<IUserMessage> SendCardsNames(this IUser user, IList<Card> cards, bool newDealtCards = false)
        {
            var prefix = newDealtCards
                ? "You were dealt"
                : $"Your current hand ({cards.Count} {(cards.Count == 1 ? "card" : "cards")}):";
            var message = cards.ToDiscordString();

            return user.SendMessageAsync($"{prefix}\n​\n{message}");
        }

        public static async Task<IUserMessage> SendCardsImages(this IUser user, IList<Card> cards, bool addPlus = false)
        {
            if (cards.IsEmpty())
                return null;

            var name = string.Join("_", cards.Select(c => c.ToShortString())) + ".png";
            if (addPlus)
                name = "Plus_" + name;

            var paths = cards.Select(c => c.ToImagePath());
            if (addPlus) paths = paths.Prepend(CardToImageHelper.PlusPath);

            if (!addPlus)
                await user.SendMessageAsync(
                    $"Your current hand ({cards.Count} {(cards.Count == 1 ? "card" : "cards")}):");

            return await user.SendFileAsync(paths.JoinImages(), name);
        }

        public static void DisposeAll(this IEnumerable<IDisposable> disposables)
        {
            var exceptions = new List<Exception>();

            foreach (var disposable in disposables)
                try
                {
                    disposable.Dispose();
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }

            if (exceptions.Any()) throw new AggregateException(exceptions);
        }

        public static string DiscordTag(this IUser user) => $"{user.Username}${user.Discriminator}";
    }
}
