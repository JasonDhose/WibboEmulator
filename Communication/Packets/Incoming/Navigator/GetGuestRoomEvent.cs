namespace WibboEmulator.Communication.Packets.Incoming.Navigator;
using WibboEmulator.Communication.Packets.Outgoing.Navigator;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal sealed class GetGuestRoomEvent : IPacketEvent
{
    public double Delay => 0;

    public void Parse(GameClient session, ClientPacket packet)
    {
        var roomID = packet.PopInt();

        var roomData = RoomManager.GenerateRoomData(roomID);
        if (roomData == null)
        {
            return;
        }

        var isLoading = packet.PopInt() == 1;
        var checkEntry = packet.PopInt() == 1;

        session.SendPacket(new GetGuestRoomResultComposer(session, roomData, isLoading, checkEntry));
    }
}
