using System.Collections.Generic;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.Game.State
{
    public abstract class GameState : IGame
    {
        protected readonly Game Game;

        protected GameState(Game game) => Game = game;

        protected GameState(GameState gameState) : this(gameState.Game)
        {
        }

        protected int CurrentPlayer => Game.CurrentPlayer;
        protected List<Card> CurrentPlayerHand => Game.CurrentPlayerHand;
        protected string CurrentPlayerName => Game.GetPlayerName(CurrentPlayer);

        public virtual bool IsFinished => false;

        public abstract Result MatchCenterRowCard(int player, Card target, params Card[] cardsToPlay);
        public abstract Result FinishMatching(int player);
        public abstract Result Draw(int player);

        public abstract Result AddCardToCenterRow(int player, Card card);

        public abstract Result Callout(int caller);
        public abstract Result CallDos(int caller);
    }
}
