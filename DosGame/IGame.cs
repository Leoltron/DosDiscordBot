using DosGame.Model;
using DosGame.Util;

namespace DosGame.State
{
    public interface IGame
    {
        Result<string> MatchCenterRowCard(int player, Card target, params Card[] cardsToPlay);
        Result<string> FinishMatching(int player);
        Result<string> Draw(int player);
        Result<string> AddCardToCenterRow(int player, Card card);
        Result<string> Callout(int caller);
        Result<string> CallDos(int caller);
    }
}