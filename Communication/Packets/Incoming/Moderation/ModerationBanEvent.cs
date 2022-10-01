using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Moderation;

namespace WibboEmulator.Communication.Packets.Incoming.Structure
{
    internal class ModerationBanEvent : IPacketEvent
    {
        public double Delay => 0;

        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetUser().HasPermission("perm_ban"))
            {
                return;
            }

            int UserId = Packet.PopInt();
            string Message = Packet.PopString();
            int Length = Packet.PopInt() * 3600;

            ModerationManager.BanUser(Session, UserId, Length, Message);
        }
    }
}