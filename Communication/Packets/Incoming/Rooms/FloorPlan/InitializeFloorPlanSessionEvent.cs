using WibboEmulator.Communication.Packets.Outgoing.Rooms.FloorPlan;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

namespace WibboEmulator.Communication.Packets.Incoming.Structure
{
    internal class InitializeFloorPlanSessionEvent : IPacketEvent
    {
        public double Delay => 0;

        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!WibboEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetUser().CurrentRoomId, out Room room))
                return;

            Session.SendPacket(new FloorPlanSendDoorComposer(room.GetGameMap().Model.DoorX, room.GetGameMap().Model.DoorY, room.GetGameMap().Model.DoorOrientation));
        }
    }
}
