using System;
using System.Linq;
using System.Collections.Generic;

namespace Daifugo
{
    public static class DaifugoFunction
    {
        /// <summary>
        /// 数字から強さへ変換
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static int NumberToRank(int number)
        {
            if (number > 13) return Card.JokerNumber;
            var rank = number - 2;
            if (rank <= 0)
            {
                rank += 13;
            }

            return rank;
        }

        /// <summary>
        /// カードの強さでソート
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static int SortRankOrderComparison(Card lhs, Card rhs)
        {
            var lRank = NumberToRank(lhs.number);
            var rRank = NumberToRank(rhs.number);
            if (lRank != rRank)
            {
                return lRank - rRank;
            }
            else
            {
                return lhs.suit - rhs.suit;
            }
        }

        /// <summary>
        /// 座先にプレイヤーIDを割り当てる
        /// </summary>
        /// <param name="playerIds"></param>
        /// <param name="seats"></param>
        public static void AssignPlayersToSeats(List<PrivateStatus> players, List<Seat> seats)
        {
            var temp = players.OrderBy(_ => Guid.NewGuid()).ToList();
            for (int i = 0; i < temp.Count; ++i)
            {
                temp[i].seat = i;
                seats[i].playerId = temp[i].id;
            }
        }

        /// <summary>
        /// カードをプレイヤーに配る
        /// </summary>
        /// <param name="deck"></param>
        /// <param name="players"></param>
        public static void DealCardsToPlayers(Deck deck, List<PrivateStatus> players)
        {
            // 最初に配るターゲットを検索
            var first = players.FirstOrDefault(p => p.roleRank == RoleRank.Daifugo);
            var firstSeat = 0;
            if (first != null)
            {
                firstSeat = first.seat;
            }

            // 座席リストを作成
            var tempPlayers = new List<PrivateStatus>(players);
            tempPlayers.Sort((lhs, rhs) => lhs.seat.CompareTo(rhs.seat));
            var seats = new List<Seat>();
            foreach (var player in tempPlayers)
            {
                var seat = new Seat();
                seat.playerId = player.id;
                seats.Add(seat);
            }

            // 座席順番に配る
            for (int i = firstSeat; true;)
            {
                var card = deck.Pop();
                if (!card.HasValue)
                {
                    break;
                }

                var target = seats[i].playerId;
                players[target].hand.Add(card.Value);
                i++;
                if (i >= seats.Count)
                {
                    i = 0;
                }
            }
        }

        /// <summary>
        /// カード交換
        /// tradingCardsはplayerAに存在するカードでないといけません
        /// 交換が成功したらtrue、失敗ならfalseを返す
        /// </summary>
        /// <param name="playerA">交換カード選択側</param>
        /// <param name="tradingCards">交換カード</param>
        /// <param name="playerB">強いカードを交換される側</param>
        /// <returns></returns>
        public static bool TradeCards(List<Card> playerA, List<Card> tradingCards, List<Card> playerB)
        {
            if (tradingCards.Count > playerB.Count)
            {
                return false;
            }

            if (tradingCards.Any(t => !playerA.Any(c => t == c)))
            {
                return false;
            }

            var temp = new List<Card>(playerB);
            temp.Sort(SortRankOrderComparison);

            var strongests = temp.GetRange(playerB.Count - tradingCards.Count, tradingCards.Count);

            foreach (var strongest in strongests)
            {
                playerB.Remove(strongest);
            }

            playerB.AddRange(tradingCards);

            foreach (var trading in tradingCards)
            {
                playerA.Remove(trading);
            }

            playerA.AddRange(strongests);

            return true;
        }

        /// <summary>
        /// フィールドに対して、有効なカードか判定
        /// </summary>
        /// <param name="playedCards"></param>
        /// <param name="playerHand"></param>
        /// <param name="fieldCards"></param>
        /// <returns></returns>
        public static bool ArePlayedCardsValid(List<Card> playedCards, List<Card> playerHand, List<Card> fieldCards)
        {
            // 出されたカードが無いのであれば無効とする
            if (playedCards == null || playedCards.Count == 0)
            {
                return false;
            }

            // フィールドカードと枚数が違うならば無効
            if (fieldCards != null
                && fieldCards.Count > 0
                && playedCards.Count != fieldCards.Count)
            {
                return false;
            }

            // 出されたカードがプレイヤーの手札に無いものがあれば無効
            if (playedCards.Any(c => !playerHand.Contains(c)))
            {
                return false;
            }

            // 出されたカード内に同じカードが複数あれば無効
            if (playedCards.GroupBy(c => c).Any(g => g.Count() >= 2))
            {
                return false;
            }

            if (fieldCards != null && fieldCards.Count == 1)
            {
                // フィールドカードが1枚ならば単純にランク比較
                var playedRank = DaifugoFunction.NumberToRank(playedCards[0].number);
                var fieldRank = DaifugoFunction.NumberToRank(fieldCards[0].number);
                return playedRank > fieldRank;
            }
            else if (AreCardsMultiple(fieldCards))
            {
                // フィールドカードが複数枚だしならば
                if (!AreCardsMultiple(playedCards))
                {
                    // 出されたカードも複数枚でなければ無効
                    return false;
                }

                // ランク比較
                var playedRank = playedCards.Min(c => DaifugoFunction.NumberToRank(c.number));
                var fieldRank = fieldCards.Min(c => DaifugoFunction.NumberToRank(c.number));
                return playedRank > fieldRank;
            }
            else if (AreCardsSequence(fieldCards))
            {
                // フィールドカードが階段だしならば
                if (!AreCardsSequence(playedCards))
                {
                    // 出されたカードも階段出しでなければ無効
                    return false;
                }

                // 階段出しランク比較
                var playedRank = GetMinimumRankOfSequenceCards(playedCards);
                var fieldRank = GetMaximumRankOfSequenceCards(fieldCards);

                return playedRank > fieldRank;
            }
            else
            {
                // フィールドカードが無ければ
                // 有効なカードか判定だけ
                if (playedCards.Count == 1
                 || AreCardsMultiple(playedCards)
                 || AreCardsSequence(playedCards))
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 階段か判定
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public static bool AreCardsSequence(List<Card> cards)
        {
            if (cards == null) return false;
            if (cards.Count < 3) return false;

            var clonedCards = new List<Card>(cards);

            var jokerIndex = clonedCards.FindIndex(c => c.suit == Suit.Joker);
            if (jokerIndex >= 0)
            {
                clonedCards.RemoveAt(jokerIndex);
            }

            var suit = clonedCards[0].suit;
            if (!clonedCards.All(c => c.suit == suit)) return false;

            clonedCards.Sort(DaifugoFunction.SortRankOrderComparison);
            bool areSequence = true;
            bool isJokerApplied = false;
            for (int i = 0; i < clonedCards.Count - 1; ++i)
            {
                var rank0 = NumberToRank(clonedCards[i].number);
                var rank1 = NumberToRank(clonedCards[i + 1].number);
                var d = rank1 - rank0;
                if (d != 1)
                {
                    if (jokerIndex == -1)
                    {
                        areSequence = false;
                        break;
                    }
                    else
                    {
                        if (d == 2 && !isJokerApplied)
                        {
                            isJokerApplied = true;
                        }
                        else
                        {
                            areSequence = false;
                            break;
                        }
                    }
                }
            }

            return areSequence;
        }

        /// <summary>
        /// 複数出しか判定
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public static bool AreCardsMultiple(List<Card> cards)
        {
            if (cards == null) return false;
            if (cards.Count < 2) return false;

            var clonedCards = new List<Card>(cards);

            var jokerIndex = clonedCards.FindIndex(c => c.suit == Suit.Joker);
            if (jokerIndex >= 0)
            {
                clonedCards.RemoveAt(jokerIndex);
            }

            var number = clonedCards[0].number;
            if (!clonedCards.All(c => c.number == number)) return false;

            return true;
        }

        /// <summary>
        /// 階段出しカードから最大ランクを取得する
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public static int GetMaximumRankOfSequenceCards(List<Card> cards)
        {
            if (!AreCardsSequence(cards))
            {
                return -1;
            }

            var clonedCards = new List<Card>(cards);

            var jokerIndex = clonedCards.FindIndex(c => c.suit == Suit.Joker);
            if (jokerIndex >= 0)
            {
                clonedCards.RemoveAt(jokerIndex);
            }

            var rank = clonedCards.Max(c => NumberToRank(c.number));

            if (jokerIndex != -1)
            {
                // ジョーカー抜きの2枚で階段？
                var areTwoCardsSequence =
                    clonedCards.Count == 2
                    && (NumberToRank(clonedCards[1].number) - NumberToRank(clonedCards[0].number) == 1);

                if (AreCardsSequence(clonedCards) || areTwoCardsSequence)
                {
                    // ジョーカー以外で階段が成立しているなら、ジョーカーが最大ランクカードを担っているとする
                    rank++;
                }
                else
                {
                    // ジョーカー以外で階段が成立していないなら、ジョーカーが中間にあるので、↑のrankそのまま
                }
            }

            return rank;
        }

        /// <summary>
        /// 階段出しカードから最小のランクを取得する
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public static int GetMinimumRankOfSequenceCards(List<Card> cards)
        {
            var max = GetMaximumRankOfSequenceCards(cards);
            if (max < 0) return -1;

            return max - cards.Count + 1;
        }
    }
}