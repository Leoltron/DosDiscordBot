using Dos.Game.Model;

namespace Dos.Game.Extensions
{
    public static class ColorExtensions
    {
        public static bool Matches(this CardColor one, CardColor other) => (one & other) != 0;
    }
}
