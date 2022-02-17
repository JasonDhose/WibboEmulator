using Butterfly.Game.Clients;
using Butterfly.Game.Moderation;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class ModerationMuteEvent : IPacketEvent
    {
        public double Delay => 0;

        public void Parse(Client Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().HasFuse("fuse_no_kick"))
            {
                return;
            }

            ModerationManager.KickUser(Session, Packet.PopInt(), Packet.PopString(), false);
        }
    }
}
