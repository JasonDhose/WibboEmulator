using Butterfly.Communication.Packets.Outgoing.Catalog;

using Butterfly.Game.Clients;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class CheckPetNameEvent : IPacketEvent
    {
        public double Delay => 0;

        public void Parse(Client Session, ClientPacket Packet)
        {
            string PetName = Packet.PopString();

            Session.SendPacket(new CheckPetNameComposer(PetName));
        }
    }
}