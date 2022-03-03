using Butterfly.Communication.Packets.Outgoing.Catalog;
using Butterfly.Game.Catalog;
using Butterfly.Game.Clients;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class GetCatalogPageEvent : IPacketEvent
    {
        public double Delay => 0;

        public void Parse(Client Session, ClientPacket Packet)
        {
            int PageId = Packet.PopInt();
            int Something = Packet.PopInt();
            string CataMode = Packet.PopString();

            ButterflyEnvironment.GetGame().GetCatalog().TryGetPage(PageId, out CatalogPage Page);
            if (Page == null || Page.MinimumRank > Session.GetUser().Rank)
            {
                return;
            }

            Session.SendPacket(new CatalogPageComposer(Page, CataMode, Session.Langue));
        }
    }
}