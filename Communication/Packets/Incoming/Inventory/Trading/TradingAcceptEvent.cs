using Butterfly.Game.GameClients;
using Butterfly.Game.Rooms;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class TradingAcceptEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Room room = ButterflyEnvironment.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
            if (room == null)
            {
                return;
            }

            Trade userTrade = room.GetUserTrade(Session.GetHabbo().Id);
            if (userTrade == null)
            {
                return;
            }

            userTrade.Accept(Session.GetHabbo().Id);
        }
    }
}