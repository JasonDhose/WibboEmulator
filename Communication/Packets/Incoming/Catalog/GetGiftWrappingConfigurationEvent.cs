using Butterfly.Communication.Packets.Outgoing.Catalog;
using Butterfly.Game.GameClients;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class GetGiftWrappingConfigurationEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendPacket(new GiftWrappingConfigurationMessageComposer());
        }
    }
}