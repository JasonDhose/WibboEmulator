using Wibbo.Communication.Packets.Outgoing.Rooms.Furni.Stickys;
using Wibbo.Game.Clients;
using Wibbo.Game.Items;
using Wibbo.Game.Rooms;

namespace Wibbo.Communication.Packets.Incoming.Structure
{
    internal class GetStickyNoteEvent : IPacketEvent
    {
        public double Delay => 250;

        public void Parse(Client Session, ClientPacket Packet)
        {
            Room room = WibboEnvironment.GetGame().GetRoomManager().GetRoom(Session.GetUser().CurrentRoomId);
            if (room == null)
            {
                return;
            }

            Item roomItem = room.GetRoomItemHandler().GetItem(Packet.PopInt());
            if (roomItem == null || roomItem.GetBaseItem().InteractionType != InteractionType.POSTIT)
            {
                return;
            }

            Session.SendPacket(new StickyNoteComposer(roomItem.Id, roomItem.ExtraData));
        }
    }
}
