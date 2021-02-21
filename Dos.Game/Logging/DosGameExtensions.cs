using System.Linq;

namespace Dos.Game.Logging
{
    public static class DosGameExtensions
    {
        public static GameSnapshot ToSnapshot(this DosGame game) =>
            new(game.CurrentPlayer?.OrderId,
                game.Players.ToDictionary(p => p.OrderId, p => p.Hand.ToArray()),
                game.CenterRow.Zip(game.CenterRowAdditional, (c, ca) => ca.Prepend(c).ToArray()).ToArray()
            );
    }
}
