using Dos.Game.Players;

namespace Dos.Game.Extensions
{
    public static class PlayerExtensions
    {
        public static bool IsActive(this Player player) =>
            player.State switch
            {
                PlayerState.WaitingForGameToStart => true,
                PlayerState.WaitingForTurn => true,
                PlayerState.Playing => true,
                _ => false
            };
    }
}
