using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Daifugo
{
    public class UpdatedStatusArgs : EventArgs
    {
        public UpdatedStatusArgs(PublicStatus publicStatus, PrivateStatus privateStatus)
        {
            PublicStatus = publicStatus;
            PrivateStatus = privateStatus;
        }

        public PublicStatus PublicStatus { get; set; }
        public PrivateStatus PrivateStatus { get; set; }
    }

    public class EndedRoundArgs : EventArgs { }
    public class EndedGameArgs : EventArgs { }

    public class Player
    {
        event EventHandler<UpdatedStatusArgs> UpdatedStatus;

        event EventHandler<EndedRoundArgs> EndedRound;

        event EventHandler<EndedGameArgs> EndedGame;


        private IClientMessageTransceiver messageTransceiver;

        private bool hasJoined = false;

        private int id = -1;

        private PublicStatus publicStatus;

        private PrivateStatus privateStatus;


        public bool HasJoined { get { return hasJoined; } }

        public int Id { get { return id; } }

        public PublicStatus PublicStatus { get { return publicStatus; } }

        public PrivateStatus PrivateStatus { get { return privateStatus; } }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Player(IClientMessageTransceiver messageTransceiver)
        {
            this.messageTransceiver = messageTransceiver;

            this.messageTransceiver.ReceivedStatus += ReceiveStatus;
            this.messageTransceiver.ReceivedEndMessage += EndGameOrRound;
        }

        /// <summary>
        /// ゲームに参加
        /// </summary>
        /// <returns></returns>
        public int JoinGame()
        {
            var task = messageTransceiver.SendJoinRequestAsync();
            task.Wait();

            if (id == -1)
            {
                hasJoined = false;
            }
            else
            {
                hasJoined = true;
            }

            return id;
        }

        /// <summary>
        /// 交換するカードを出す
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public ResultOfPlaying TradeCards(List<Card> cards)
        {
            return ResultOfPlaying.Accepted;
        }

        /// <summary>
        /// カードを出す
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public ResultOfPlaying PlayCards(List<Card> cards)
        {
            return ResultOfPlaying.Accepted;
        }



        /// <summary>
        /// ゲーム状態を受け取る
        /// </summary>
        private void ReceiveStatus(object sender, ReceivedStatusArgs args)
        {
            publicStatus = args.PublicStatus;
            privateStatus = args.PrivateStatus;
            UpdatedStatus?.Invoke(this, new UpdatedStatusArgs(publicStatus, privateStatus));
        }

        /// <summary>
        /// ゲーム終了
        /// </summary>
        private void EndGameOrRound(object sender, ReceivedEndMessageArgs args)
        {
            switch (args.EndMessage)
            {
                case EndMessage.NotEnd:
                    break;
                case EndMessage.EndRound:
                    EndedRound?.Invoke(this, new EndedRoundArgs());
                    break;
                case EndMessage.EndGame:
                    EndedGame?.Invoke(this, new EndedGameArgs());
                    break;
            }
        }
    }
}