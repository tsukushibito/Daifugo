using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Daifugo
{
    public class SingleProcessMessageTransceiver : IServerMessageTransceiver, IClientMessageTransceiver
    {
        static SingleProcessMessageTransceiver()
        {
            Server = new SingleProcessMessageTransceiver();
            Clients = new List<SingleProcessMessageTransceiver>();
        }

        public static SingleProcessMessageTransceiver Server { get; private set; }

        public static List<SingleProcessMessageTransceiver> Clients { get; private set; }

        /// <summary>
        /// ゲーム参加リクエスト受け取り時イベント
        /// </summary>
        public event EventHandler<ReceivedJoinRequestArgs> ReceivedJoinRequest;

        /// <summary>
        /// カード受け取り時イベント
        /// </summary>
        public event EventHandler<ReceivedCardsArgs> ReceivedCards;

        /// <summary>
        /// プレイヤーIDを通知
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public Task SendPlayerIdAsync(int playerId)
        {
            var task = new Task(() =>
            {
                if (playerId < 0) return;
                if (playerId >= Clients.Count) return;
                Clients[playerId].ReceivedPlayerId?.Invoke(this, new ReceivedPlayerIdArgs(playerId));
            });
            task.RunSynchronously();
            return task;
        }

        /// <summary>
        /// ゲーム情報通知
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="publicStatus"></param>
        /// <param name="privateStatus"></param>
        /// <returns></returns>
        public Task SendStatusAsync(
            int playerId,
            PublicStatus publicStatus,
            PrivateStatus privateStatus)
        {
            var task = new Task(() =>
            {
                if (playerId < 0) return;
                if (playerId >= Clients.Count) return;
                Clients[playerId].ReceivedStatus?.Invoke(this, new ReceivedStatusArgs(publicStatus, privateStatus));
            });
            task.RunSynchronously();
            return task;
        }

        /// <summary>
        /// カード受理結果を通知
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public Task SendResultOfPlayingAsync(int playerId, ResultOfPlaying result)
        {
            var task = new Task(() =>
            {
                if (playerId < 0) return;
                if (playerId >= Clients.Count) return;
                Clients[playerId].ReceivedResultOfPlaying?.Invoke(this, new ReceivedResultOfPlayingArgs(result));
            });
            task.RunSynchronously();
            return task;
        }

        /// <summary>
        /// ゲーム終了か通知
        /// Server2Client
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task SendEndMessageAsync(int playerId, EndMessage message)
        {
            var task = new Task(() =>
            {
                if (playerId < 0) return;
                if (playerId >= Clients.Count) return;
                Clients[playerId].ReceivedEndMessage?.Invoke(this, new ReceivedEndMessageArgs(message));
            });
            task.RunSynchronously();
            return task;
        }

        public event EventHandler<ReceivedPlayerIdArgs> ReceivedPlayerId;

        public event EventHandler<ReceivedStatusArgs> ReceivedStatus;

        public event EventHandler<ReceivedResultOfPlayingArgs> ReceivedResultOfPlaying;

        public event EventHandler<ReceivedEndMessageArgs> ReceivedEndMessage;

        /// <summary>
        /// 参加リクエスト
        /// </summary>
        public Task SendJoinRequestAsync()
        {
            var task = new Task(() =>
            {
                Server.ReceivedJoinRequest?.Invoke(this, new ReceivedJoinRequestArgs());
            });
            task.RunSynchronously();
            return task;
        }

        /// <summary>
        /// カード通知
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="cards"></param>
        /// <returns>受理結果</returns>
        public Task SendCardsAsync(int playerId, List<Card> cards)
        {
            var task = new Task(() =>
            {
                Server.ReceivedCards?.Invoke(this, new ReceivedCardsArgs(playerId, cards));
            });
            task.RunSynchronously();
            return task;
        }
    }
}