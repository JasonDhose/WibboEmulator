using Butterfly.Communication.Packets.Outgoing.Groups;
using Butterfly.Game.Clients;
using Butterfly.Game.Rooms;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class GetGroupCreationWindowEvent : IPacketEvent
    {
        public double Delay => 0;

        public void Parse(Client Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetUser() == null)
            {
                return;
            }

            List<RoomData> ValidRooms = new List<RoomData>();
            foreach (int RoomId in Session.GetUser().UsersRooms)
            {
                RoomData Data = ButterflyEnvironment.GetGame().GetRoomManager().GenerateRoomData(RoomId);
                if (Data == null)
                    continue;

                if (Data.Group == null)
                {
                    ValidRooms.Add(Data);
                }
            }

            Session.SendPacket(new GroupCreationWindowComposer(ValidRooms));
        }
    }
}