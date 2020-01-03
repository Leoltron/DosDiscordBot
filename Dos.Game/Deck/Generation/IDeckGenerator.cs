using Dos.Game.Model;

namespace Dos.Game.Deck.Generation
{
    public interface IDeckGenerator
    {
        Card[] Generate();
    }
}
