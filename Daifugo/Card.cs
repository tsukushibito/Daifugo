using System;
namespace Daifugo
{
    /// <summary>
    /// カード
    /// </summary>
    public struct Card : IComparable<Card>, IEquatable<Card>
    {
        /// <summary>
        /// ジョーカーの数字
        /// </summary>
        public const int JokerNumber = 14;

        /// <summary>
        /// 番号順ソート用Comparison
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static int SortNumberOrderComparison(Card lhs, Card rhs)
        {
            if (lhs.number != rhs.number)
            {
                return lhs.number - rhs.number;
            }
            else
            {
                return lhs.suit - rhs.suit;
            }
        }

        /// <summary>
        /// スート順ソート用Comparison
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static int SortSuitOrderComparison(Card lhs, Card rhs)
        {
            if (lhs.suit != rhs.suit)
            {
                return lhs.suit - rhs.suit;
            }
            else
            {
                return lhs.number - rhs.number;
            }
        }



        /// <summary>
        /// スート
        /// </summary>
        public readonly Suit suit;

        /// <summary>
        /// 数字
        /// </summary>
        public readonly int number;



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="suit"></param>
        /// <param name="number"></param>
        public Card(Suit suit, int number)
        {
            this.suit = suit;
            this.number = suit == Suit.Joker ? JokerNumber : number;
        }



        /// <summary>
        /// 文字列変換
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Card: [" + suit.ToString() + "] " + number.ToString();
        }

        /// <summary>
        /// ハッシュ値変換
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return number.GetHashCode() ^ suit.GetHashCode();
        }



        // 以下、比較用メソッド

        int IComparable<Card>.CompareTo(Card other)
        {
            if (suit != other.suit)
            {
                return suit - other.suit;
            }
            else
            {
                return number - other.number;
            }
        }

        bool IEquatable<Card>.Equals(Card other)
        {
            return suit == other.suit && number == other.number;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Card)) return false;
            var other_ = (Card)obj;
            return suit == other_.suit && number == other_.number;
        }

        public static bool operator ==(Card lhs, Card rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Card lhs, Card rhs)
        {
            return (lhs == rhs);
        }
    }
}
