using System.Linq;
using Xunit;
using Daifugo;

namespace DaifugoTest
{
    public class DeckTest
    {
        [Fact]
        public void Pop_DefaultDeck_DecreasedCards()
        {
            var deck = new Deck();
            var cardCount = deck.Cards.Count();
            deck.Pop();
            var decreasedCardCount = deck.Cards.Count();
            var diff = cardCount - decreasedCardCount;
            Assert.Equal(1, diff);
        }

        [Fact]
        public void Pop_EmptyDeck_ReturnNull()
        {
            var deck = new Deck();
            var cardCount = deck.Cards.Count();
            for (int i = 0; i < 53; ++i)
            {
                deck.Pop();
            }

            var popped = deck.Pop();
            Assert.False(popped.HasValue);
        }
    }
}
