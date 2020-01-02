using DosGame.Model;

namespace DosGame.Deck.Generation
{
    public interface IDeckGenerator
    {
        Card[] Generate();
    }
}
