using Butterfly.Game.Clients;
using Butterfly.Game.Rooms;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class TradingConfirmEvent : IPacketEvent
    {
        public void Parse(Client Session, ClientPacket Packet)
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

            userTrade.CompleteTrade(Session.GetHabbo().Id);
        }
    }
}