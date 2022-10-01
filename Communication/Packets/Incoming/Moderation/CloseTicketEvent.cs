using WibboEmulator.Games.GameClients;

namespace WibboEmulator.Communication.Packets.Incoming.Structure
{
    internal class CloseTicketEvent : IPacketEvent
    {
        public double Delay => 0;

        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetUser() == null || !Session.GetUser().HasPermission("perm_mod"))
            {
                return;
            }

            int Result = Packet.PopInt();
            Packet.PopInt();
            int TicketId = Packet.PopInt();

            WibboEnvironment.GetGame().GetModerationManager().CloseTicket(Session, Packet.PopInt(), Result);
        }
    }
}
