using System.Collections.Generic;
using Dos.Game.Model;
using Dos.Utils;

namespace Dos.Game.State.Base
{
    public abstract class GameState : IGame
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

        protected int CurrentPlayer => Game.CurrentPlayer;
        protected List<Card> CurrentPlayerHand => Game.CurrentPlayerHand;
        protected string CurrentPlayerName => Game.GetPlayerName(CurrentPlayer);

        public virtual bool IsFinished => false;

        public abstract Result MatchCenterRowCard(int player, Card target, params Card[] cardsToPlay);
        public abstract Result EndTurn(int player);
        public abstract Result Draw(int player);

        public abstract Result AddCardToCenterRow(int player, Card card);

        public abstract Result Callout(int caller);
        public abstract Result CallDos(int caller);
    }
}
