namespace WibboEmulator.Communication.Packets.Incoming.Messenger;
using WibboEmulator.Games.GameClients;

internal sealed class RemoveBuddyEvent : IPacketEvent
{
    public double Delay => 0;

    public void Parse(GameClient session, ClientPacket packet)
    {
        if (session.User.Messenger == null)
        {
            return;
        }

        var count = packet.PopInt();

        if (count > 200)
        {
            count = 200;
        }

        int friendId;
        for (var index = 0; index < count; index++)
        {
            friendId = packet.PopInt();
            session.User.Messenger.DestroyFriendship(friendId);
        }
    }
}
