using System.Linq;
using System.Collections.Generic;
using Xunit;
using Daifugo;

namespace DaifugoTest
{
    public class MessageTransceiverTest
    {
        public static TheoryData<IServerMessageTransceiver, IClientMessageTransceiver> SingleProcess =>
            new TheoryData<IServerMessageTransceiver, IClientMessageTransceiver>
            {
                { SingleProcessMessageTransceiver.Server, SingleProcessMessageTransceiver.CreateClient() },
            };

        [Theory]
        [MemberData(nameof(SingleProcess))]
        public void SendJoinRequestAsync_None_ServerGetConnectionId(IServerMessageTransceiver server, IClientMessageTransceiver client)
        {
            string connectionId = null;
            server.ReceivedJoinRequest += (sender, args) =>
            {
                connectionId = args.ConnectionId;
            };

            client.SendJoinRequestAsync();

            Assert.False(string.IsNullOrEmpty(connectionId));
        }

        [Theory]
        [MemberData(nameof(SingleProcess))]
        public void SendPlayerId_None_ClientReceivedPlayerId(IServerMessageTransceiver server, IClientMessageTransceiver client)
        {
            const int TestPlayerId = 0;

            server.ReceivedJoinRequest += (sender, args) =>
            {
                server.SendPlayerIdAsync(args.ConnectionId, TestPlayerId);
            };

            int playerId = -1;
            client.ReceivedPlayerId += (sender, args) =>
            {
                playerId = args.PlayerId;
            };

            client.SendJoinRequestAsync();

            Assert.Equal(TestPlayerId, playerId);
        }

        [Theory]
        [MemberData(nameof(SingleProcess))]
        public void SendStatusAsync_SendStatus_ClientReceivedStatus(IServerMessageTransceiver server, IClientMessageTransceiver client)
        {
            string connectionId = null;
            server.ReceivedJoinRequest += (_, args) =>
            {
                connectionId = args.ConnectionId;
            };

            client.SendJoinRequestAsync();

            var publicStatus = new PublicStatus
            {
                field = new List<Card> { new Card(Suit.Joker, 0), },
                phase = Phase.Trading,
                turn = 1,
                hasFlowed = true,
                isElevenBack = false,
                isKakumei = false,
                isShibari = false,
                playerStatuses = new List<PublicPlayerStatus>
                {
                    new PublicPlayerStatus
                    {
                        id = 0,
                        seat = 2,
                        roleRank = RoleRank.Heimin,
                        cardCount = 3,
                        hasPassed = false,
                    }
                },
            };

            var privateStatus = new PrivateStatus
            {
                id = 0,
                seat = 2,
                roleRank = RoleRank.Heimin,
                hand = new List<Card> { new Card(Suit.Spades, 1), new Card(Suit.Spades, 2), new Card(Suit.Spades, 3) },
                tradingCardCount = 0,
                hasPassed = false,
            };

            PublicStatus actualPublicStatus = null;
            PrivateStatus actualPrivateStatus = null;
            client.ReceivedStatus += (_, args) =>
            {
                actualPublicStatus = args.PublicStatus;
                actualPrivateStatus = args.PrivateStatus;
            };

            server.SendStatusAsync(connectionId, publicStatus, privateStatus);

            var isEqual =
                publicStatus.field.SequenceEqual(actualPublicStatus.field) &&
                publicStatus.phase == actualPublicStatus.phase &&
                publicStatus.turn == actualPublicStatus.turn &&
                publicStatus.hasFlowed == actualPublicStatus.hasFlowed &&
                publicStatus.isElevenBack == actualPublicStatus.isElevenBack &&
                publicStatus.isKakumei == actualPublicStatus.isKakumei &&
                publicStatus.isShibari == actualPublicStatus.isShibari &&
                publicStatus.playerStatuses.SequenceEqual(actualPublicStatus.playerStatuses);

            isEqual &=
                privateStatus.id == actualPrivateStatus.id &&
                privateStatus.seat == actualPrivateStatus.seat &&
                privateStatus.roleRank == actualPrivateStatus.roleRank &&
                privateStatus.hand.SequenceEqual(actualPrivateStatus.hand) &&
                privateStatus.tradingCardCount == actualPrivateStatus.tradingCardCount &&
                privateStatus.hasPassed == actualPrivateStatus.hasPassed;

            Assert.True(isEqual);
        }

        [Theory]
        [MemberData(nameof(SingleProcess))]
        public void SendResultOfPlayingAsync_SendResult_ClientReceivedResult(IServerMessageTransceiver server, IClientMessageTransceiver client)
        {
            string connectionId = null;
            server.ReceivedJoinRequest += (_, args) =>
            {
                connectionId = args.ConnectionId;
            };

            ResultOfPlaying result = ResultOfPlaying.NotAccepted;
            client.ReceivedResultOfPlaying += (_, args) =>
            {
                result = args.Result;
            };

            client.SendJoinRequestAsync();

            server.SendResultOfPlayingAsync(connectionId, ResultOfPlaying.Accepted);

            Assert.Equal(ResultOfPlaying.Accepted, result);
        }

        [Theory]
        [MemberData(nameof(SingleProcess))]
        public void SendEndMessageAsync(IServerMessageTransceiver server, IClientMessageTransceiver client)
        {
            string connectionId = null;
            server.ReceivedJoinRequest += (_, args) =>
            {
                connectionId = args.ConnectionId;
            };

            EndMessage msg = EndMessage.NotEnd;
            client.ReceivedEndMessage += (_, args) =>
            {
                msg = args.EndMessage;
            };

            client.SendJoinRequestAsync();

            server.SendEndMessageAsync(connectionId, EndMessage.EndGame);

            Assert.Equal(EndMessage.EndGame, msg);
        }
    }
}