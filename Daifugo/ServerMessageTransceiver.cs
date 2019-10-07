using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Daifugo
{
    public class ReceivedJoinRequestArgs : EventArgs
    {
        public ReceivedJoinRequestArgs(string id)
        {
            ConnectionId = id;
        }

        public string ConnectionId { get; set; }
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
        /// <param name="connectionId"></param>
        /// <param name="playerId"></param>
        /// <returns></returns>
        Task SendPlayerIdAsync(string connectionId, int playerId);

        /// <summary>
        /// ゲーム情報通知
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="publicStatus"></param>
        /// <param name="privateStatus"></param>
        /// <returns></returns>
        Task SendStatusAsync(
            string connectionId,
            PublicStatus status,
            PrivateStatus privateStatus);

        /// <summary>
        /// カード受理結果を通知
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        Task SendResultOfPlayingAsync(string connectionId, ResultOfPlaying result);

        /// <summary>
        /// ゲーム終了か通知
        /// Server2Client
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendEndMessageAsync(string connectionId, EndMessage message);
    }
}