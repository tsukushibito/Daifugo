using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Daifugo
{
    public class ReceivedJoinRequestArgs : EventArgs
    {
    }

    public class ReceivedCardsArgs : EventArgs
    {
        public ReceivedCardsArgs(int playerId, List<Card> cards)
        {
            PlayerId = playerId;
            Cards = cards;
        }

        public int PlayerId { get; set; }
        public List<Card> Cards { get; set; }
    }

    /// <summary>
    /// サーバー用メッセージ通知受信処理インターフェース
    /// </summary>
    public interface IServerMessageTransceiver
    {
        /// <summary>
        /// ゲーム参加リクエスト受け取り時イベント
        /// </summary>
        event EventHandler<ReceivedJoinRequestArgs> ReceivedJoinRequest;

        /// <summary>
        /// カード受け取り時イベント
        /// </summary>
        event EventHandler<ReceivedCardsArgs> ReceivedCards;

        /// <summary>
        /// プレイヤーIDを通知
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        Task SendPlayerIdAsync(int playerId);

        /// <summary>
        /// ゲーム情報通知
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="publicStatus"></param>
        /// <param name="privateStatus"></param>
        /// <returns></returns>
        Task SendStatusAsync(
            int playerId,
            PublicStatus status,
            PrivateStatus privateStatus);

        /// <summary>
        /// カード受理結果を通知
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        Task SendResultOfCards(int playerId, int result);

        /// <summary>
        /// ゲーム終了か通知
        /// Server2Client
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="isEnd"></param>
        /// <returns></returns>
        Task SendEndingAsync(int playerId, int isEnd);
    }
}