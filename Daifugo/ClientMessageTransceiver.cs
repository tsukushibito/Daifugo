using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Daifugo
{
    public class ReceivedPlayerIdArgs : EventArgs
    {
        public ReceivedPlayerIdArgs(int playerId)
        {
            PlayerId = playerId;
        }

        public int PlayerId { get; set; }
    }

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

    public class ReceivedResultOfPlayingArgs : EventArgs
    {
        public ReceivedResultOfPlayingArgs(ResultOfPlaying result)
        {
            Result = result;
        }

        public ResultOfPlaying Result { get; set; }
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
        event EventHandler<ReceivedPlayerIdArgs> ReceivedPlayerId;

        event EventHandler<ReceivedStatusArgs> ReceivedStatus;

        event EventHandler<ReceivedResultOfPlayingArgs> ReceivedResultOfPlaying;

        event EventHandler<ReceivedEndMessageArgs> ReceivedEndMessage;

        /// <summary>
        /// 参加リクエスト
        /// </summary>
        Task SendJoinRequestAsync();

        /// <summary>
        /// カード通知
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="cards"></param>
        Task SendCardsAsync(int playerId, List<Card> cards);
    }
}