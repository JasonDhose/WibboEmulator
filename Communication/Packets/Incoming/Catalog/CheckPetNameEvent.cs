using WibboEmulator.Communication.Packets.Outgoing.Catalog;

using WibboEmulator.Games.GameClients;

namespace WibboEmulator.Communication.Packets.Incoming.Structure
{
    internal class CheckPetNameEvent : IPacketEvent
    {
        public double Delay => 0;

        public void Parse(GameClient Session, ClientPacket Packet)
        {
            string PetName = Packet.PopString();

            Session.SendPacket(new CheckPetNameComposer(PetName));
        }
    }
}