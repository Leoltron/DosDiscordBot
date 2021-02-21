using Dos.Game.Model;
using Dos.Game.Players;
using Dos.Utils;

namespace Dos.Game.State.Base
{
    public abstract class GameState : IDosGame
    {
        protected readonly DosGame Game;

        protected GameState(DosGame game)
        {
            Game = game;
        }

        protected GameState(GameState gameState) : this(gameState.Game)
        {
        }

        protected GameConfig Config => Game.Config;

        protected Player CurrentPlayer => Game.CurrentPlayer;

        public virtual bool IsFinished => false;
        public virtual bool CanMatch => false;
        public virtual bool CanAdd => false;
        public virtual bool CanDraw => false;

        public abstract Result MatchCenterRowCard(Player player, Card target, params Card[] cardsToPlay);
        public abstract Result EndTurn(Player player);
        public abstract Result Draw(Player player);

        public abstract Result AddCardToCenterRow(Player player, Card card);

        public abstract Result Callout(Player caller, Player target);
        public abstract Result CallDos(Player caller);
        public abstract Result SwapWith(Player caller, Player target);
    }
}
