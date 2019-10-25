using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Daifugo
{
    /// <summary>
    /// フェーズ
    /// </summary>
    public enum Phase
    {
        AcceptingPlayer,
        Trading,
        BeforPlaying,
        AfterPlaying,
        End,
    }

    /// <summary>
    /// 座席
    /// </summary>
    public class Seat
    {
        public int playerId = -1;

        public Seat Clone()
        {
            var clone = new Seat();
            clone.playerId = playerId;
            return clone;
        }
    }

    /// <summary>
    /// 全プレイヤー向け共通情報
    /// </summary>
    public class PublicStatus
    {
        public int round;
        public List<Card> field = new List<Card>();
        public Phase phase;
        public int turn = -1;
        public bool hasFlowed = false;
        public bool isElevenBack = false;
        public bool isKakumei = false;
        public bool isShibari = false;
        public List<PublicPlayerStatus> playerStatuses = new List<PublicPlayerStatus>();

        public PublicStatus Clone()
        {
            var clone = new PublicStatus();
            clone.round = round;
            foreach (var card in field)
            {
                field.Add(card);
            }
            clone.phase = phase;
            clone.hasFlowed = hasFlowed;
            clone.isElevenBack = isElevenBack;
            clone.isKakumei = isKakumei;
            clone.isShibari = isShibari;
            foreach (var playerStatus in playerStatuses)
            {
                playerStatuses.Add(playerStatus.Clone());
            }

            return clone;
        }
    }

    /// <summary>
    /// 公開プレイヤー情報
    /// </summary>
    public class PublicPlayerStatus
    {
        public int id = -1;
        public int seat = -1;
        public RoleRank roleRank = RoleRank.Heimin;
        public int cardCount = -1;
        public bool hasPassed = false;

        public PublicPlayerStatus Clone()
        {
            var clone = new PublicPlayerStatus();
            clone.id = id;
            clone.seat = seat;
            clone.roleRank = roleRank;
            clone.cardCount = cardCount;
            clone.hasPassed = hasPassed;

            return clone;
        }
    }

    /// <summary>
    /// 特定プレイヤー専用情報
    /// </summary>
    public class PrivateStatus
    {
        public int id = -1;
        public int seat = -1;
        public RoleRank roleRank = RoleRank.Heimin;
        public List<Card> hand = new List<Card>();
        public int tradingCardCount = 0;
        public bool hasPassed = false;

        public PrivateStatus Clone()
        {
            var clone = new PrivateStatus();

            clone.id = id;
            clone.seat = seat;
            clone.roleRank = roleRank;
            foreach (var card in hand)
            {
                hand.Add(card);
            }
            clone.tradingCardCount = tradingCardCount;
            clone.hasPassed = hasPassed;

            return clone;
        }
    }
}