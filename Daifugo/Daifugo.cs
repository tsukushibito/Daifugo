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
        private readonly IServerMessageTransceiver messageTransceiver;

        private readonly Dictionary<int, string> playerIdToConnectionIdMap = new Dictionary<int, string>();

        private Deck deck;

        private List<LocalRule> rules;

        private Stack<List<Card>> fieldStack;

        private List<Card> flowedCards;

        private List<Seat> seats;

        private List<PrivateStatus> players;

        private int playerCount;

        private Phase phase = Phase.AcceptingPlayer;

        private bool isStoppedAcceptingPlayer;

        public ReadOnlyCollection<LocalRule> LocalRules { get { return rules.AsReadOnly(); } }

        public int Round { get; private set; }

        public int Turn { get; private set; }

        public bool HasEnded { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Daifugo(IServerMessageTransceiver messageTransceiver)
        {
            this.messageTransceiver = messageTransceiver;

            InitializeGame(5, null);

            this.messageTransceiver.ReceivedJoinRequest += (sender, args) =>
            {
                var playerId = LetPlayerJoin();
                var connectionId = args.ConnectionId;
                if(playerId != -1)
                {
                    playerIdToConnectionIdMap.Add(playerId, connectionId);
                }
                var task = messageTransceiver.SendPlayerIdAsync(connectionId, playerId);
                task.RunSynchronously();
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

            HasEnded = true;
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
        private int LetPlayerJoin()
        {
            if (players.Count >= playerCount)
            {
                return -1;
            }

            var id = players.Count;
            var player = new PrivateStatus
            {
                id = id,
                hand = new List<Card>()
            };
            players.Add(player);

            return id;
        }

        /// <summary>
        /// 公開情報を作成
        /// </summary>
        /// <returns></returns>
        private PublicStatus MakePublicStatus()
        {
            var publicStatus = new PublicStatus
            {
                field = fieldStack.FirstOrDefault(),
                phase = Phase.Trading,
                turn = 0,
                hasFlowed = false,
                isElevenBack = false,
                isKakumei = false,
                isShibari = false,
                playerStatuses = new List<PublicPlayerStatus>()
            };
            foreach (var player in players)
            {
                var playerStatus = new PublicPlayerStatus
                {
                    id = player.id,
                    seat = player.seat,
                    roleRank = player.roleRank,
                    cardCount = player.hand.Count,
                    hasPassed = player.hasPassed
                };
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
                        opponent = players.FirstOrDefault(p => p.roleRank == RoleRank.Daihinmin);
                        break;
                    case RoleRank.Fugo:
                        opponent = players.FirstOrDefault(p => p.roleRank == RoleRank.Hinmin);
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
                if (playerId != Turn)
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
                player.tradingCardCount = player.roleRank switch
                {
                    RoleRank.Daifugo => 2,
                    RoleRank.Fugo => 1,
                    _ => 0,
                };
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
                string connection;
                playerIdToConnectionIdMap.TryGetValue(player.id, out connection);
                var task = messageTransceiver.SendStatusAsync(connection, publicStatus, player);
                tasks.Add(task);
            }
            foreach (var task in tasks)
            {
                task.Wait();
            }

            // クライアントから交換カード受け取り待ち
            while (players.Any(p => p.tradingCardCount > 0))
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
                string connection;
                playerIdToConnectionIdMap.TryGetValue(player.id, out connection);
                var task = messageTransceiver.SendStatusAsync(connection, publicStatus, player);
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
                string connection;
                playerIdToConnectionIdMap.TryGetValue(player.id, out connection);
                var task = messageTransceiver.SendStatusAsync(connection, publicStatus, player);
                tasks.Add(task);
            }
            foreach (var task in tasks)
            {
                task.Wait();
            }

            // ゲーム終了か各プレイヤーに通知
            var isEnd = players.Count(p => p.hand.Count == 0) <= 1;

            if (isEnd)
            {
                phase = Phase.End;
            }
            else
            {
                // ターンを進める
                var now = Turn;
                do
                {
                    Turn++;
                    if (Turn >= playerCount)
                    {
                        Turn = 0;
                    }

                    if (Turn == now)
                    {
                        // System.Diagnostics.Debug.Assert(false);
                        phase = Phase.End;
                    }
                    if (phase == Phase.End) return;

                } while (players[Turn].hand.Count == 0);

                phase = Phase.BeforPlaying;
            }
        }
    }
}