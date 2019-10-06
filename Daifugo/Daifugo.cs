using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Daifugo
{
    /// <summary>
    /// 大富豪管理クラス
    /// </summary>
    public class Daifugo
    {
        private IServerMessageTransceiver messageTransceiver;

        private Deck deck;

        private List<LocalRule> rules;

        private Stack<List<Card>> fieldStack;

        private List<Card> flowedCards;

        private List<Seat> seats;

        private List<PrivateStatus> players;

        private int playerCount = 0;

        private int round = 0;

        private int turn = 0;

        private Phase phase = Phase.AcceptingPlayer;

        private bool isStoppedAcceptingPlayer = false;

        private bool hasEnded = false;


        public ReadOnlyCollection<LocalRule> LocalRules { get { return rules.AsReadOnly(); } }

        public int Round { get { return round; } }

        public int Turn { get { return turn; } }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Daifugo(IServerMessageTransceiver messageTransceiver)
        {
            this.messageTransceiver = messageTransceiver;

            InitializeGame(5, null);

            this.messageTransceiver.ReceivedJoinRequest += (sender, args) =>
            {
                AddPlayer();
            };

            this.messageTransceiver.ReceivedCards += (sender, args) =>
            {
                ReceivedCard(args.PlayerId, args.Cards);
            };
        }

        /// <summary>
        /// 終了
        /// </summary>
        public void Exit()
        {
            phase = Phase.End;
        }

        /// <summary>
        /// ゲームの参加を締め切る
        /// </summary>
        public void StopAcceptingPlayer()
        {
            isStoppedAcceptingPlayer = true;
        }

        /// <summary>
        /// 実行
        /// </summary>
        /// <returns></returns>
        public void Run()
        {
            while (true)
            {
                switch (phase)
                {
                    case Phase.AcceptingPlayer:
                        UpdateAcceptingPlayer();
                        break;

                    case Phase.Trading:
                        UpdateTrading();
                        break;

                    case Phase.BeforPlaying:
                        UpdateBeforPlaying();
                        break;

                    case Phase.AfterPlaying:
                        UpdateAfterPlaying();
                        break;
                    default:
                        break;
                }

                if (phase == Phase.End)
                {
                    break;
                }
            }

            hasEnded = true;
        }

        /// <summary>
        /// ゲームの初期化
        /// </summary>
        /// <param name="playerCount"></param>
        /// <param name="rules"></param>
        private void InitializeGame(int playerCount, List<LocalRule> rules)
        {
            this.rules = rules;
            this.playerCount = playerCount;

            deck = new Deck();

            fieldStack = new Stack<List<Card>>();
            flowedCards = new List<Card>();
            seats = new List<Seat>();

            for (int i = 0; i < playerCount; ++i)
            {
                seats.Add(new Seat());
            }

            players = new List<PrivateStatus>();
        }

        /// <summary>
        /// プレイヤーを追加
        /// 参加不可の場合-1を返す
        /// </summary>
        /// <returns>プレイヤーID</returns>
        private int AddPlayer()
        {
            if (players.Count >= playerCount)
            {
                return -1;
            }

            var id = players.Count;
            var player = new PrivateStatus();
            player.id = id;
            player.hand = new List<Card>();
            players.Add(player);

            return id;
        }

        /// <summary>
        /// 公開情報を作成
        /// </summary>
        /// <returns></returns>
        private PublicStatus MakePublicStatus()
        {
            var publicStatus = new PublicStatus();
            publicStatus.field = fieldStack.FirstOrDefault();
            publicStatus.phase = Phase.Trading;
            publicStatus.turn = 0;
            publicStatus.hasFlowed = false;
            publicStatus.isElevenBack = false;
            publicStatus.isKakumei = false;
            publicStatus.isShibari = false;
            publicStatus.playerStatuses = new List<PublicPlayerStatus>();
            foreach (var player in players)
            {
                var playerStatus = new PublicPlayerStatus();
                playerStatus.id = player.id;
                playerStatus.seat = player.seat;
                playerStatus.roleRank = player.roleRank;
                playerStatus.cardCount = player.hand.Count;
                playerStatus.hasPassed = player.hasPassed;
                publicStatus.playerStatuses.Add(playerStatus);
            }

            return publicStatus;
        }

        /// <summary>
        /// カード情報を受け取ったときの処理
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="cards"></param>
        /// <returns></returns>
        private ResultOfPlaying ReceivedCard(int playerId, List<Card> cards)
        {
            switch (phase)
            {
                case Phase.Trading:
                    return ReceivedTradingCards(playerId, cards);

                case Phase.BeforPlaying:
                    return ReceivedPlayingCards(playerId, cards);

                default:
                    break;
            }

            return ResultOfPlaying.Accepted;
        }

        /// <summary>
        /// 交換カード受け取り時処理
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="cards"></param>
        /// <returns></returns>
        private ResultOfPlaying ReceivedTradingCards(int playerId, List<Card> cards)
        {
            var result = ResultOfPlaying.Accepted;
            do
            {
                // カード枚数チェック
                if (players[playerId].tradingCardCount != cards.Count)
                {
                    result = ResultOfPlaying.NotAccepted;
                    break;
                }

                // 交換相手を特定
                var player = players[playerId];
                PrivateStatus opponent = null;
                switch (player.roleRank)
                {
                    case RoleRank.Daifugo:
                        opponent = players.Where(p => p.roleRank == RoleRank.Daihinmin).FirstOrDefault();
                        break;
                    case RoleRank.Fugo:
                        opponent = players.Where(p => p.roleRank == RoleRank.Hinmin).FirstOrDefault();
                        break;
                    default:
                        // logger(LogLevel.Error, "Player rank must be Daifugo or Fugo!");
                        break;
                }

                // 交換相手存在チェック
                if (opponent == null)
                {
                    // logger( LogLevel.Error, "Not found opponent!");
                    result = ResultOfPlaying.NotAccepted;
                    break;
                }

                // 交換処理
                if (!DaifugoFunction.TradeCards(player.hand, cards, opponent.hand))
                {
                    // 不正なカードが指定されていた
                    // logger( LogLevel.Error, "Received card is invalid!");
                    result = ResultOfPlaying.NotAccepted;
                    break;
                }

            } while (false);
            players[playerId].tradingCardCount = 0;

            return result;
        }

        /// <summary>
        /// ターンのプレイヤーからカードを受け取り時の処理
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="cards"></param>
        /// <returns></returns>
        private ResultOfPlaying ReceivedPlayingCards(int playerId, List<Card> cards)
        {
            ResultOfPlaying result = ResultOfPlaying.Accepted;

            do
            {
                if (playerId != turn)
                {
                    // logger( LogLevel.Error, "Not player turn. Player(" + playerId + ")");
                    result = ResultOfPlaying.NotAccepted;
                    // フェーズを進めずリターン
                    return result;
                }

                var playerHand = players[playerId].hand;

                var field = fieldStack.FirstOrDefault();
                if (DaifugoFunction.ArePlayedCardsValid(cards, playerHand, field))
                {
                    // フィールドスタックにカードを追加して
                    fieldStack.Push(cards);
                    // パスフラグをリセット
                    players.ForEach(p => p.hasPassed = false);
                    result = ResultOfPlaying.Accepted;
                }
                else
                {
                    players[playerId].hasPassed = true;

                    if (players.All(p => p.hasPassed || p.hand.Count == 0))
                    {
                        // 全員パスなら、場を流してパスフラグをリセット
                        while (fieldStack.Count > 0)
                        {
                            flowedCards.AddRange(fieldStack.Pop());
                        }
                        players.ForEach(p => p.hasPassed = false);
                    }
                    result = ResultOfPlaying.NotAccepted;
                }

            } while (false);

            phase = Phase.AfterPlaying;

            return result;
        }

        /// <summary>
        /// プレイヤー参加受付時処理
        /// </summary>
        /// <returns></returns>
        private void UpdateAcceptingPlayer()
        {
            while (players.Count < playerCount && !isStoppedAcceptingPlayer)
            {
            }

            if (isStoppedAcceptingPlayer)
            {
                // TODO: プレイヤー数を満たしていない場合はAIプレイヤーを参加させる
            }

            // プレイヤーの席順決定
            DaifugoFunction.AssignPlayersToSeats(players, seats);

            // フェーズをカード交換に移行
            phase = Phase.Trading;
        }

        /// <summary>
        /// カード交換時処理
        /// </summary>
        /// <returns></returns>
        private void UpdateTrading()
        {
            // ランクに応じたカード交換枚数を設定
            foreach (var player in players)
            {
                switch (player.roleRank)
                {
                    case RoleRank.Daifugo:
                        player.tradingCardCount = 2;
                        break;
                    case RoleRank.Fugo:
                        player.tradingCardCount = 1;
                        break;
                    default:
                        player.tradingCardCount = 0;
                        break;
                }
            }

            // デックをシャッフル
            deck.Shuffle();

            // カードを各プレイヤーに配布
            DaifugoFunction.DealCardsToPlayers(deck, players);

            // ゲーム状況を各プレイヤーに通知
            var publicStatus = MakePublicStatus();
            var tasks = new List<Task>();
            foreach (var player in players)
            {
                var task = messageTransceiver.SendStatusAsync(player.id, publicStatus, player);
                tasks.Add(task);
            }
            foreach (var task in tasks)
            {
                task.Wait();
            }

            // クライアントから交換カード受け取り待ち
            while (players.Count(p => p.tradingCardCount > 0) > 0)
            {
                if (phase == Phase.End) return;
            }

            phase = Phase.BeforPlaying;
        }

        /// <summary>
        /// プレイヤーの提出カード受け取り前処理
        /// </summary>
        /// <returns></returns>
        private void UpdateBeforPlaying()
        {
            // 手札を各プレイヤーに通知
            var publicStatus = MakePublicStatus();
            var tasks = new List<Task>();
            foreach (var player in players)
            {
                var task = messageTransceiver.SendStatusAsync(player.id, publicStatus, player);
                tasks.Add(task);
            }
            foreach (var task in tasks)
            {
                task.Wait();
            }

            // ターンのプレイヤーからのカード提出待ち
            while (phase == Phase.BeforPlaying)
            {
                if (phase == Phase.End) return;
            }

            phase = Phase.AfterPlaying;
        }

        /// <summary>
        /// プレイヤーの提出カード受け取りご処理
        /// </summary>
        /// <returns></returns>
        private void UpdateAfterPlaying()
        {
            // ゲーム情報を各プレイヤーに通知
            var publicStatus = MakePublicStatus();
            var tasks = new List<Task>();
            foreach (var player in players)
            {
                var task = messageTransceiver.SendStatusAsync(player.id, publicStatus, player);
                tasks.Add(task);
            }
            foreach (var task in tasks)
            {
                task.Wait();
            }

            // ゲーム終了か各プレイヤーに通知
            var isEnd = false;
            if (players.Count(p => p.hand.Count == 0) <= 1)
            {
                // 手札が残っているプレイヤーが1人以下なら終了
                isEnd = true;
            }

            if (isEnd)
            {
                phase = Phase.End;
            }
            else
            {
                // ターンを進める
                var now = turn;
                do
                {
                    turn++;
                    if (turn >= playerCount)
                    {
                        turn = 0;
                    }

                    if (turn == now)
                    {
                        // System.Diagnostics.Debug.Assert(false);
                        phase = Phase.End;
                    }
                    if (phase == Phase.End) return;

                } while (players[turn].hand.Count == 0);

                phase = Phase.BeforPlaying;
            }
        }
    }
}