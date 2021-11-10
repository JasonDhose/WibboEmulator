using Butterfly.Communication.Packets.Outgoing;
using Butterfly.Game.GameClients;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class FindNewFriendsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            int RoomId = 447654;

            ServerPacket Response = new ServerPacket(ServerPacketHeader.ROOM_FORWARD);
            Response.WriteInteger(RoomId);
            Session.SendPacket(Response);
        }
    }
}
