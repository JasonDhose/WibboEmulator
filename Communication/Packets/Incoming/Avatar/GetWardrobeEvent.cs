using Wibbo.Communication.Packets.Outgoing.Avatar;
using Wibbo.Game.Clients;

namespace Wibbo.Communication.Packets.Incoming.Structure
{
    internal class GetWardrobeEvent : IPacketEvent
    {
        public double Delay => 0;

        public void Parse(Client Session, ClientPacket Packet)
        {
            Session.SendPacket(new WardrobeComposer(Session.GetUser().GetWardrobeComponent().GetWardrobes()));
        }
    }
}