namespace WibboEmulator.Communication.Packets.Incoming.Catalog;
using WibboEmulator.Communication.Packets.Outgoing.Catalog;

using WibboEmulator.Games.GameClients;

internal sealed class CheckPetNameEvent : IPacketEvent
{
    public double Delay => 0;

    public void Parse(GameClient Session, ClientPacket packet)
    {
        var petName = packet.PopString(16);

        Session.SendPacket(new CheckPetNameComposer(petName));
    }
}
