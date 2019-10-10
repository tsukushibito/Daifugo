using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Daifugo
{
    public class SingleProcessMessageTransceiver : IServerMessageTransceiver, IClientMessageTransceiver
    {

        public static SingleProcessMessageTransceiver Server { get; private set; }

        public static List<SingleProcessMessageTransceiver> Clients { get; private set; }

        private static readonly Dictionary<string, SingleProcessMessageTransceiver> _connectedClientTable;

        public static SingleProcessMessageTransceiver CreateClient()
        {
            var client = new SingleProcessMessageTransceiver();
            Clients.Add(client);
            return client;
        }

        private static string CreateConnection(SingleProcessMessageTransceiver client)
        {
            var id = Guid.NewGuid();
            var idStr = id.ToString();
            _connectedClientTable.Add(idStr, client);
            return idStr;
        }

        static SingleProcessMessageTransceiver()
        {
            Server = new SingleProcessMessageTransceiver();
            Clients = new List<SingleProcessMessageTransceiver>();
            _connectedClientTable = new Dictionary<string, SingleProcessMessageTransceiver>();
        }

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
        /// <param name="connectionId"></param>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public Task SendPlayerIdAsync(string connectionId, int playerId)
        {
            var task = new Task(() =>
            {
                if (string.IsNullOrEmpty(connectionId)) return;
                SingleProcessMessageTransceiver client;
                if (_connectedClientTable.TryGetValue(connectionId, out client))
                {
                    client.ReceivedPlayerId?.Invoke(this, new ReceivedPlayerIdArgs(playerId));
                }
            });
            task.RunSynchronously();
            return task;
        }

        /// <summary>
        /// ゲーム情報通知
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="publicStatus"></param>
        /// <param name="privateStatus"></param>
        /// <returns></returns>
        public Task SendStatusAsync(
            string connectionId,
            PublicStatus publicStatus,
            PrivateStatus privateStatus)
        {
            var task = new Task(() =>
            {
                if (string.IsNullOrEmpty(connectionId)) return;
                SingleProcessMessageTransceiver client;
                if (_connectedClientTable.TryGetValue(connectionId, out client))
                {
                    client.ReceivedStatus?.Invoke(this, new ReceivedStatusArgs(publicStatus, privateStatus));
                }
            });
            task.RunSynchronously();
            return task;
        }

        /// <summary>
        /// カード受理結果を通知
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public Task SendResultOfPlayingAsync(string connectionId, ResultOfPlaying result)
        {
            var task = new Task(() =>
            {
                if (string.IsNullOrEmpty(connectionId)) return;
                SingleProcessMessageTransceiver client;
                if (_connectedClientTable.TryGetValue(connectionId, out client))
                {
                    client.ReceivedResultOfPlaying?.Invoke(this, new ReceivedResultOfPlayingArgs(result));
                }
            });
            task.RunSynchronously();
            return task;
        }

        /// <summary>
        /// ゲーム終了か通知
        /// Server2Client
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task SendEndMessageAsync(string connectionId, EndMessage message)
        {
            var task = new Task(() =>
            {
                if (string.IsNullOrEmpty(connectionId)) return;
                SingleProcessMessageTransceiver client;
                if (_connectedClientTable.TryGetValue(connectionId, out client))
                {
                    client.ReceivedEndMessage?.Invoke(this, new ReceivedEndMessageArgs(message));
                }
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
                var id = _connectedClientTable.Where(p => p.Value == this).Select(p => p.Key).FirstOrDefault();
                if (string.IsNullOrEmpty(id))
                {
                    id = CreateConnection(this);
                }
                Server.ReceivedJoinRequest?.Invoke(this, new ReceivedJoinRequestArgs(id));
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