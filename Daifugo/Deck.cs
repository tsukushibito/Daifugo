using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Daifugo
{
    /// <summary>
    /// デッキ
    /// </summary>
    public class Deck
    {
        /// <summary>
        /// カードリスト
        /// </summary>
        private List<Card> cards;

        /// <summary>
        /// カードプロパティ
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<Card> Cards { get { return cards.AsReadOnly(); } }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Deck()
        {
            cards = new List<Card>();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                if (suit == Suit.Joker) continue;

                for (int i = 1; i <= 13; ++i)
                {
                    cards.Add(new Card(suit, i));
                }
            }

            cards.Add(new Card(Suit.Joker, Card.JokerNumber));
        }


        /// <summary>
        /// シャッフル
        /// カードリストのカードをランダムに並び替えます
        /// </summary>
        public void Shuffle()
        {
            cards = cards.OrderBy(_ => Guid.NewGuid()).ToList();
        }

        /// <summary>
        /// ソート
        /// カードリストを指定のComparisonでソートします
        /// </summary>
        /// <param name="comparison"></param>
        public void Sort(Comparison<Card> comparison = null)
        {
            if (comparison == null)
            {
                comparison = Card.SortSuitOrderComparison;
            }
            cards.Sort(comparison);
        }

        /// <summary>
        /// ソート
        /// カードリストをスート順でソートします
        /// </summary>
        public void SortSuitOrder()
        {
            cards.Sort(Card.SortSuitOrderComparison);
        }

        /// <summary>
        /// ソート
        /// カードリストを数字順でソートします
        /// </summary>
        public void SortNumberOrder()
        {
            cards.Sort(Card.SortNumberOrderComparison);
        }

        /// <summary>
        /// ポップ
        /// カードリストの先頭カードを取り出します
        /// </summary>
        /// <returns></returns>
        public Card? Pop()
        {
            if (cards == null || cards.Count == 0)
            {
                return null;
            }

            var top = cards[0];

            cards.RemoveAt(0);

            return top;
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var msg = "Deck: \n";
            for (int i = 0; i < cards.Count; ++i)
            {
                msg += "[" + i + "]" + cards[i] + "\n";
            }

            return msg;
        }
    }

}