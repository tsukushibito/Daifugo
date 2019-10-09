using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Daifugo;

namespace DaifugoTest
{

    public class CardTest
    {
        public static TheoryData<Card, Card, bool> EqualsTestData =>
            new TheoryData<Card, Card, bool>
            {
                {new Card(Suit.Spades, 1), new Card(Suit.Spades, 1), true},
                {new Card(Suit.Joker, 1), new Card(Suit.Joker, Card.JokerNumber), true},
                {new Card(Suit.Spades, 1), new Card(Suit.Spades, 2), false},
                {new Card(Suit.Spades, 1), new Card(Suit.Hearts, 1), false},
                {new Card(Suit.Spades, 1), new Card(Suit.Hearts, 2), false},
            };

        [Theory]
        [MemberData(nameof(EqualsTestData))]
        public void Equals_Cards_ReturnTrueIfSame(Card lhs, Card rhs, bool expected)
        {
            var result = lhs == rhs;
            Assert.Equal(expected, result);
        }

        public static TheoryData<List<Card>, List<Card>> SortNumberOrderComparisonTestData =>
            new TheoryData<List<Card>, List<Card>>
            {
                {
                    new List<Card> { new Card(Suit.Spades, 3), new Card(Suit.Spades, 1), new Card(Suit.Spades, 2) },
                    new List<Card>{ new Card(Suit.Spades, 1), new Card(Suit.Spades, 2), new Card(Suit.Spades, 3) }
                },
                {
                    new List<Card> { new Card(Suit.Hearts, 1), new Card(Suit.Spades, 1), new Card(Suit.Spades, 2) },
                    new List<Card>{ new Card(Suit.Spades, 1), new Card(Suit.Hearts, 1), new Card(Suit.Spades, 2) }
                },
            };

        [Theory]
        [MemberData(nameof(SortNumberOrderComparisonTestData))]
        public void SortNumberOrderComparison_SameSuitCards_SortedByNumber(List<Card> cards, List<Card> sorted)
        {
            cards.Sort(Card.SortNumberOrderComparison);
            var result = cards.SequenceEqual(sorted);
            Assert.True(result);
        }
    }
}