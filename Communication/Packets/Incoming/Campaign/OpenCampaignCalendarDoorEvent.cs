namespace WibboEmulator.Communication.Packets.Incoming.Campaign;
using WibboEmulator.Communication.Packets.Outgoing.Campaign;
using WibboEmulator.Games.GameClients;

internal sealed class OpenCampaignCalendarDoorEvent : IPacketEvent
{
    public double Delay => 0;

    public void Parse(GameClient Session, ClientPacket packet)
    {
        _ = packet.PopString();

        _ = packet.PopInt();

        Session.SendPacket(new CampaignCalendarDoorOpenedComposer(true, "", "/album1584/LOL.gif", ""));
    }
}
