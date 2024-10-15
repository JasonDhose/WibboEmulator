namespace WibboEmulator.Communication.Packets.Incoming.Rooms.Action;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Action;
using WibboEmulator.Games.GameClients;

internal sealed class IgnoreUserEvent : IPacketEvent
{
    public double Delay => 250;

    public void Parse(GameClient session, ClientPacket packet)
    {
        if (session.User == null)
        {
            return;
        }

        if (session.User.Room == null)
        {
            return;
        }

        var userName = packet.PopString(16);

        var gameclient = GameClientManager.GetClientByUsername(userName);
        if (gameclient == null)
        {
            return;
        }

        var user = gameclient.User;
        if (user == null || session.User.MutedUsers.Contains(user.Id))
        {
            return;
        }

        session.User.MutedUsers.Add(user.Id);

        session.SendPacket(new IgnoreStatusComposer(1, userName));
    }
}
