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

        [Fact]
        public void AssignPlayersToSeats_PlayersAndSeats_SeatIdIsAssigned()
        {
            var players = new List<PrivateStatus>
            {
                new PrivateStatus() { id = 0 },
                new PrivateStatus() { id = 1 },
                new PrivateStatus() { id = 2 },
                new PrivateStatus() { id = 3 },
                new PrivateStatus() { id = 4 },
            };
            var seats = new List<Seat>
            {
                new Seat(),
                new Seat(),
                new Seat(),
                new Seat(),
                new Seat(),
            };

            DaifugoFunction.AssignPlayersToSeats(players, seats);
            foreach (var player in players)
            {
                var assigned = seats[player.seat].playerId;
                Assert.Equal(player.id, assigned);
            }
        }

        [Fact]
        public void DealCardsToPlayers_5Players_HandCountIs11or10()
        {
            var deck = new Deck();
            var players = new List<PrivateStatus>
            {
                new PrivateStatus() { id = 0, seat = 0, roleRank = RoleRank.Heimin },
                new PrivateStatus() { id = 1, seat = 1, roleRank = RoleRank.Heimin },
                new PrivateStatus() { id = 2, seat = 2, roleRank = RoleRank.Heimin },
                new PrivateStatus() { id = 3, seat = 3, roleRank = RoleRank.Heimin },
                new PrivateStatus() { id = 4, seat = 4, roleRank = RoleRank.Heimin },
            };

            DaifugoFunction.DealCardsToPlayers(deck, players);
            var handCount0 = players[0].hand.Count;
            var handCount1 = players[1].hand.Count;
            var handCount2 = players[2].hand.Count;
            var handCount3 = players[3].hand.Count;
            var handCount4 = players[4].hand.Count;
            var result = handCount0 == 11 && handCount1 == 11 && handCount2 == 11 && handCount3 == 10 && handCount4 == 10;
            Assert.True(result);
        }

        [Fact]
        public void DealCardsToPlayers_5Players_ValidHands()
        {
            var deck = new Deck();
            var players = new List<PrivateStatus>
            {
                new PrivateStatus() { id = 0, seat = 0, roleRank = RoleRank.Heimin },
                new PrivateStatus() { id = 1, seat = 1, roleRank = RoleRank.Heimin },
                new PrivateStatus() { id = 2, seat = 2, roleRank = RoleRank.Heimin },
                new PrivateStatus() { id = 3, seat = 3, roleRank = RoleRank.Heimin },
                new PrivateStatus() { id = 4, seat = 4, roleRank = RoleRank.Heimin },
            };

            DaifugoFunction.DealCardsToPlayers(deck, players);
            var cards = new List<Card>();
            foreach (var player in players)
            {
                foreach (var card in player.hand)
                {
                    var exists = cards.Exists(c => c == card);
                    Assert.False(exists);
                    cards.Add(card);
                }
            }
        }

        [Fact]
        public void TradeCards_ValidCards_ReturnTrue()
        {
            var playerAHand = new List<Card>
            {
                 new Card(Suit.Spades, 1), new Card(Suit.Spades, 2), new Card(Suit.Spades, 3), new Card(Suit.Spades, 4)
            };
            var tradeCards = new List<Card>
            {
                 new Card(Suit.Spades, 1), new Card(Suit.Spades, 2)
            };
            var playerBHand = new List<Card>
            {
                 new Card(Suit.Hearts, 1), new Card(Suit.Hearts, 2), new Card(Suit.Hearts, 3), new Card(Suit.Hearts, 4)
            };
            var result = DaifugoFunction.TradeCards(playerAHand, tradeCards, playerBHand);
            Assert.True(result);
        }

        [Fact]
        public void TradeCards_InvalidCards_ReturnFalse()
        {
            var playerAHand = new List<Card>
            {
                 new Card(Suit.Spades, 1), new Card(Suit.Spades, 2), new Card(Suit.Spades, 3), new Card(Suit.Spades, 4)
            };
            var tradeCards = new List<Card>
            {
                 new Card(Suit.Spades, 1), new Card(Suit.Clubs, 2)
            };
            var playerBHand = new List<Card>
            {
                 new Card(Suit.Hearts, 1), new Card(Suit.Hearts, 2), new Card(Suit.Hearts, 3), new Card(Suit.Hearts, 4)
            };
            var result = DaifugoFunction.TradeCards(playerAHand, tradeCards, playerBHand);
            Assert.False(result);
        }

        [Fact]
        public void TradeCards_ValidCards_TradedCards()
        {
            var playerAHand = new List<Card>
            {
                 new Card(Suit.Spades, 1), new Card(Suit.Spades, 2), new Card(Suit.Spades, 3), new Card(Suit.Spades, 4)
            };
            var tradeCards = new List<Card>
            {
                 new Card(Suit.Spades, 3), new Card(Suit.Spades, 4)
            };
            var playerBHand = new List<Card>
            {
                 new Card(Suit.Hearts, 1), new Card(Suit.Hearts, 2), new Card(Suit.Hearts, 3), new Card(Suit.Hearts, 4)
            };
            DaifugoFunction.TradeCards(playerAHand, tradeCards, playerBHand);
            playerAHand.Sort(DaifugoFunction.SortRankOrderComparison);
            playerBHand.Sort(DaifugoFunction.SortRankOrderComparison);

            var expectedA = new List<Card>
            {
                 new Card(Suit.Spades, 1), new Card(Suit.Spades, 2), new Card(Suit.Hearts, 1), new Card(Suit.Hearts, 2)
            };
            expectedA.Sort(DaifugoFunction.SortRankOrderComparison);
            Assert.True(expectedA.SequenceEqual(playerAHand));

            var expectedB = new List<Card>
            {
                 new Card(Suit.Spades, 3), new Card(Suit.Spades, 4), new Card(Suit.Hearts, 3), new Card(Suit.Hearts, 4)
            };
            expectedB.Sort(DaifugoFunction.SortRankOrderComparison);
            Assert.True(expectedB.SequenceEqual(playerBHand));
        }

        public static TheoryData<List<Card>, List<Card>, bool> ArePlayedCardsValidTestData =>
            new TheoryData<List<Card>, List<Card>, bool>
            {
                // �t�B�[���h������1���o��
                {
                    new List<Card>{ new Card(Suit.Spades, 13)},
                    null,
                    true
                },
                // �t�B�[���h������1���o�������J�[�h
                {
                    new List<Card>{ new Card(Suit.Spades, 4)},
                    null,
                    false
                },
                // �t�B�[���h1����1���o��
                {
                    new List<Card>{ new Card(Suit.Spades, 13)},
                    new List<Card>{ new Card(Suit.Spades, 3)},
                    true
                },
                // �t�B�[���h1����1���o�������J�[�h
                {
                    new List<Card>{ new Card(Suit.Spades, 13)},
                    new List<Card>{ new Card(Suit.Hearts, 1)},
                    false
                },
                // �t�B�[���h2����1���o��
                {
                    new List<Card>{ new Card(Suit.Spades, 13)},
                    new List<Card>{ new Card(Suit.Spades, 3), new Card(Suit.Hearts, 3)},
                    false
                },
                // �t�B�[���h2����2���o��
                {
                    new List<Card>{ new Card(Suit.Spades, 13), new Card(Suit.Hearts, 13)},
                    new List<Card>{ new Card(Suit.Spades, 3), new Card(Suit.Hearts, 3)},
                    true
                },
                // �t�B�[���h2���̃W���[�J�[����2���o��
                {
                    new List<Card>{ new Card(Suit.Spades, 13), new Card(Suit.Joker, Card.JokerNumber)},
                    new List<Card>{ new Card(Suit.Spades, 3), new Card(Suit.Hearts, 3)},
                    true
                },
                // �t�B�[���h2���̃W���[�J�[����2���o�������g�ݍ��킹
                {
                    new List<Card>{ new Card(Suit.Spades, 13), new Card(Suit.Joker, Card.JokerNumber)},
                    new List<Card>{ new Card(Suit.Spades, 1), new Card(Suit.Hearts, 1)},
                    false
                },
                // �t�B�[���h�K�i�̊K�i����
                {
                    new List<Card>{ new Card(Suit.Spades, 13), new Card(Suit.Spades, 1), new Card(Suit.Spades, 2)},
                    new List<Card>{ new Card(Suit.Spades, 3), new Card(Suit.Spades, 4), new Card(Suit.Spades, 5)},
                    true
                },
                // �t�B�[���h�K�i�̊K�i���������J�[�h
                {
                    new List<Card>{ new Card(Suit.Spades, 13), new Card(Suit.Spades, 1), new Card(Suit.Spades, 2), new Card(Suit.Joker, 0)},
                    new List<Card>{ new Card(Suit.Spades, 3), new Card(Suit.Spades, 4), new Card(Suit.Spades, 5)},
                    false
                },
                // �t�B�[���h�K�i�̊K�i���������J�[�h
                {
                    new List<Card>{ new Card(Suit.Spades, 12), new Card(Suit.Spades, 13), new Card(Suit.Spades, 1)},
                    new List<Card>{ new Card(Suit.Hearts, 10), new Card(Suit.Hearts, 11), new Card(Suit.Hearts, 12)},
                    false
                },
                // �t�B�[���h�K�i�̃W���[�J�[���K�i����
                {
                    new List<Card>{ new Card(Suit.Joker, 0), new Card(Suit.Spades, 13), new Card(Suit.Spades, 1)},
                    new List<Card>{ new Card(Suit.Hearts, 10), new Card(Suit.Hearts, 11), new Card(Suit.Hearts, 12)},
                    true
                },
                // �t�B�[���h�K�i�̃W���[�J�[���K�i����
                {
                    new List<Card>{ new Card(Suit.Joker, 0), new Card(Suit.Spades, 1), new Card(Suit.Spades, 2)},
                    new List<Card>{ new Card(Suit.Hearts, 11), new Card(Suit.Hearts, 12), new Card(Suit.Hearts, 13)},
                    true
                },
                // �t�B�[���h�K�i�̃W���[�J�[���K�i���������J�[�h
                {
                    new List<Card>{ new Card(Suit.Joker, 0), new Card(Suit.Spades, 12), new Card(Suit.Spades, 13)},
                    new List<Card>{ new Card(Suit.Hearts, 10), new Card(Suit.Hearts, 11), new Card(Suit.Hearts, 12)},
                    false
                },
            };

        [Theory]
        [MemberData(nameof(ArePlayedCardsValidTestData))]
        public void ArePlayedCardsValid_PlayedCards_ReturnTrueIfCardsAreValid(List<Card> playedCards, List<Card> fieldCards, bool expected)
        {
            var playerHand = new List<Card>
            {
                new Card(Suit.Spades, 12),
                new Card(Suit.Spades, 13),
                new Card(Suit.Hearts, 13),
                new Card(Suit.Spades, 1),
                new Card(Suit.Spades, 2),
                new Card(Suit.Joker, Card.JokerNumber)
            };
            var actual = DaifugoFunction.ArePlayedCardsValid(playedCards, playerHand, fieldCards);
            Assert.Equal(expected, actual);
        }
    }
}
