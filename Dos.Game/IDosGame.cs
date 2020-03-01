using Dos.Game.Model;
using Dos.Game.Players;
using Dos.Utils;

namespace Dos.Game
{
    public interface IDosGame
    {
        Result MatchCenterRowCard(Player player, Card target, params Card[] cardsToPlay);
        Result EndTurn(Player player);
        Result Draw(Player player);
        Result AddCardToCenterRow(Player player, Card card);
        Result Callout(Player caller, Player target);
        Result CallDos(Player caller);
        Result SwapWith(Player caller, Player target);
    }
}
