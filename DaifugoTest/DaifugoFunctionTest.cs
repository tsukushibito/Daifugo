using System.Linq;
using System.Collections.Generic;
using Xunit;
using Daifugo;

namespace DaifugoTest
{
    public class DaifugoFunctionTest
    {
        [Theory]
        [InlineData(3, 1)]
        [InlineData(4, 2)]
        [InlineData(13, 11)]
        [InlineData(1, 12)]
        [InlineData(2, 13)]
        [InlineData(14, 14)]
        [InlineData(99, 14)]
        public void NumberToRank_Number_Rank(int number, int rank)
        {
            var actual = DaifugoFunction.NumberToRank(number);

            Assert.Equal(rank, actual);
        }


        public static TheoryData<List<Card>, List<Card>> SortRankOrderComparisonTestData =>
            new TheoryData<List<Card>, List<Card>>
            {
                {
                    new List<Card> { new Card(Suit.Spades, 1), new Card(Suit.Spades, 2), new Card(Suit.Spades, 3) },
                    new List<Card>{ new Card(Suit.Spades, 3), new Card(Suit.Spades, 1), new Card(Suit.Spades, 2) }
                },
                {
                    new List<Card> { new Card(Suit.Hearts, 1), new Card(Suit.Spades, 1), new Card(Suit.Spades, 2) },
                    new List<Card>{ new Card(Suit.Spades, 1), new Card(Suit.Hearts, 1), new Card(Suit.Spades, 2) }
                },
                {
                    new List<Card> { new Card(Suit.Joker, 1), new Card(Suit.Spades, 1), new Card(Suit.Spades, 2) },
                    new List<Card>{ new Card(Suit.Spades, 1), new Card(Suit.Spades, 2), new Card(Suit.Joker, 1) }
                },
            };

        [Theory]
        [MemberData(nameof(SortRankOrderComparisonTestData))]
        public void SortRankOrderComparison_Cards_Sorted(List<Card> cards, List<Card> expected)
        {
            cards.Sort(DaifugoFunction.SortRankOrderComparison);

            Assert.True(cards.SequenceEqual(expected));
        }

        public void AssignPlayersToSeats_()
        {
            var players = new List<PrivateStatus>()
            {
                new PrivateStatus() { id = 0 },
                new PrivateStatus() { id = 1 },
                new PrivateStatus() { id = 2 },
                new PrivateStatus() { id = 3 },
                new PrivateStatus() { id = 4 },
            };
            var seats = new List<Seat>();

            DaifugoFunction.AssignPlayersToSeats(players, seats);
        }

        public void DealCardsToPlayers_()
        {
        }

        public void TradeCards_()
        {
        }

        public void ArePlayedCardsValid_()
        {
        }

        public void AreCardsSequence_()
        {
        }

        public void AreCardsMultiple_()
        {
        }

        public void GetMaximumRankOfSequenceCards_()
        {
        }

        public void GetMinimumRankOfSequenceCards_()
        {
        }
    }
}
