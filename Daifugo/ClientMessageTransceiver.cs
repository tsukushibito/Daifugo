using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Daifugo
{
    public class ReceivedStatusArgs : EventArgs
    {
        public ReceivedStatusArgs(PublicStatus publicStatus, PrivateStatus privateStatus)
        {
            PublicStatus = publicStatus;
            PrivateStatus = privateStatus;
        }

        public PublicStatus PublicStatus { get; set; }
        public PrivateStatus PrivateStatus { get; set; }
    }

    public class ReceivedEndMessageArgs : EventArgs
    {
        public ReceivedEndMessageArgs(EndMessage message)
        {
            EndMessage = message;
        }

        public EndMessage EndMessage { get; set; }
    }

    /// <summary>
    /// クライアント用メッセージ通知受信処理インターフェース
    /// </summary>
    public interface IClientMessageTransceiver
    {
        event EventHandler<ReceivedStatusArgs> ReceivedStatus;
        event EventHandler<ReceivedEndMessageArgs> ReceivedEndMessage;

        /// <summary>
        /// 参加リクエスト
        /// </summary>
        /// <returns>プレイヤーID</returns>
        Task<int> SendJoinRequestAsync();

        /// <summary>
        /// カード通知
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="cards"></param>
        /// <returns>受理結果</returns>
        Task<ResultOfPlayingCards> SendCardsAsync(int playerId, List<Card> cards);
    }
}