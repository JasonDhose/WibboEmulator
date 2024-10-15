namespace WibboEmulator.Communication.Packets.Incoming.Avatar;
using WibboEmulator.Games.GameClients;

internal sealed class SaveWardrobeOutfitEvent : IPacketEvent
{
    public double Delay => 250;

    public void Parse(GameClient session, ClientPacket packet)
    {
        var slotId = packet.PopInt();
        var look = packet.PopString();
        var gender = packet.PopString(1);

        session.User.WardrobeComponent.AddWardrobe(look, gender, slotId);
    }
}
