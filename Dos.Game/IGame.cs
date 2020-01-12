using Dos.Game.Model;
using Dos.Utils;

namespace Dos.Game
{
    public interface IGame
    {
        Result MatchCenterRowCard(int player, Card target, params Card[] cardsToPlay);
        Result EndTurn(int player);
        Result Draw(int player);
        Result AddCardToCenterRow(int player, Card card);
        Result Callout(int caller);
        Result CallDos(int caller);
    }
}
