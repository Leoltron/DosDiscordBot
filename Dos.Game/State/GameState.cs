using System.Collections.Generic;
using Dos.Game.Model;
using Dos.Game.Util;

namespace Dos.Game.State
{
    public abstract class GameState : IGame
    {
        protected readonly Game Game;

        protected GameState(Game game)
        {
            Game = game;
        }

        protected GameState(GameState gameState) : this(gameState.Game)
        {
        }

        protected int CurrentPlayer => Game.CurrentPlayer;
        protected List<Card> CurrentPlayerHand => Game.CurrentPlayerHand;
        protected string CurrentPlayerName => Game.GetPlayerName(CurrentPlayer);

        public abstract Result<string> MatchCenterRowCard(int player, Card target, params Card[] cardsToPlay);
        public abstract Result<string> FinishMatching(int player);
        public abstract Result<string> Draw(int player);
        
        public abstract Result<string> AddCardToCenterRow(int player, Card card);

        public abstract Result<string> Callout(int caller);
        public abstract Result<string> CallDos(int caller);
    }
}
