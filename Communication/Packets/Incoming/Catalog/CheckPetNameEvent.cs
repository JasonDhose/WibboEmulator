using Butterfly.Communication.Packets.Outgoing.Catalog;

using Butterfly.Game.GameClients;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class CheckPetNameEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            string PetName = Packet.PopString();

            Session.SendPacket(new CheckPetNameMessageComposer(PetName));
        }
    }
}