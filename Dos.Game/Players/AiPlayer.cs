using System.Linq;
using Dos.Game.Extensions;
using Dos.Game.Model;
using Dos.Game.State;
using Dos.Utils;

namespace Dos.Game.Players
{
    public abstract class AiPlayer : Player
    {
        protected AiPlayer(int orderId) : base(orderId, AiHelpers.GenerateBotName(orderId), true)
        {
        }

        protected AiPlayer(int orderId, string name) : base(orderId, name, true)
        {
        }

        public override void Play(DosGame game)
        {
            while (game.CurrentPlayer == this && !game.CurrentState.IsFinished)
            {
                var moveResult = ChooseMove(game);

                if (moveResult.IsSuccess)
                    continue;

                if (moveResult.HasMessage)
                    game.PublicLog(moveResult.Message);
                game.PublicLog($"*Something went wrong, {Name} quits*");
                game.Quit(this);
                return;
            }

            if (CanBeCalledOut)
            {
                game.CallDos(this);
            }
        }

        private Result ChooseMove(DosGame game)
        {
            if (game.CurrentState.CanMatch)
            {
                var match = MakeMatch(game);
                if (match != null)
                {
                    var (matchers, target) = match.Value;
                    game.PublicLog($"`Match {matchers.ToLogString()} on {target.ToString()}` ({target.MatchWith(matchers).Message()})");
                    return game.MatchCenterRowCard(this, target, matchers);
                }
            }

            if (game.CurrentState.CanAdd)
            {
                var card = ChooseCardForAdding(game);
                game.PublicLog($"`Add {card}`");
                return game.AddCardToCenterRow(this, card);
            }

            if (game.CurrentState.CanDraw)
            {
                game.PublicLog("`Draw`");
                return game.Draw(this);
            }

            if (game.CurrentState is TriggeredSwapGameState)
            {
                var swapTarget = game.Players
                                     .Where(p => p != this && p.IsActive())
                                     .OrderBy(p => p.Hand.Count)
                                     .First();
                game.PublicLog($"`Swapping with {swapTarget}`");
                return game.SwapWith(this, swapTarget);
            }

            game.PublicLog("`Done`");
            return game.EndTurn(this);
        }

        protected abstract (Card[] matchers, Card target)? MakeMatch(DosGame game);
        protected abstract Card ChooseCardForAdding(DosGame game);
    }
}
