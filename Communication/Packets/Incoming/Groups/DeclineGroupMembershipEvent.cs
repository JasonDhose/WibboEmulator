namespace WibboEmulator.Communication.Packets.Incoming.Groups;
using WibboEmulator.Communication.Packets.Outgoing.Groups;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Groups;

internal sealed class DeclineGroupMembershipEvent : IPacketEvent
{
    public double Delay => 100;

    public void Parse(GameClient session, ClientPacket packet)
    {
        var groupId = packet.PopInt();
        var userId = packet.PopInt();

        if (!GroupManager.TryGetGroup(groupId, out var group))
        {
            return;
        }

        if (session.User.Id != group.CreatorId && !group.IsAdmin(session.User.Id))
        {
            return;
        }

        if (!group.HasRequest(userId))
        {
            return;
        }

        group.HandleRequest(userId, false);
        session.SendPacket(new UnknownGroupComposer(group.Id, userId));
    }
}
