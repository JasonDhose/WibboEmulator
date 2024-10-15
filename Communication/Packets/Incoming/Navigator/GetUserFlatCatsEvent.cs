namespace WibboEmulator.Communication.Packets.Incoming.Navigator;
using WibboEmulator.Communication.Packets.Outgoing.Navigator;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Navigators;

internal sealed class GetUserFlatCatsEvent : IPacketEvent
{
    public double Delay => 0;

    public void Parse(GameClient Session, ClientPacket packet)
    {
        var categories = NavigatorManager.FlatCategories;

        Session.SendPacket(new UserFlatCatsComposer(categories, Session.User.Rank));
    }
}
