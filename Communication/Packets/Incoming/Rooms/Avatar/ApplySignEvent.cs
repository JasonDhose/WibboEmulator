using Wibbo.Game.Clients;
using Wibbo.Game.Rooms;

namespace Wibbo.Communication.Packets.Incoming.Structure
{
    internal class ApplySignEvent : IPacketEvent
    {
        public double Delay => 250;

        public void Parse(Client Session, ClientPacket Packet)
        {
            Room room = WibboEnvironment.GetGame().GetRoomManager().GetRoom(Session.GetUser().CurrentRoomId);
            if (room == null)
            {
                return;
            }

            RoomUser roomUserByUserId = room.GetRoomUserManager().GetRoomUserByUserId(Session.GetUser().Id);
            if (roomUserByUserId == null)
            {
                return;
            }

            roomUserByUserId.Unidle();

            int num = Packet.PopInt();
            if (roomUserByUserId.Statusses.ContainsKey("sign"))
            {
                roomUserByUserId.RemoveStatus("sign");
            }

            roomUserByUserId.SetStatus("sign", Convert.ToString(num));
            roomUserByUserId.UpdateNeeded = true;
        }
    }
}
