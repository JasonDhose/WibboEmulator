using Butterfly.Game.Clients;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class SSOTicketEvent : IPacketEvent
    {
        public double Delay => 5000;

        public void Parse(Client Session, ClientPacket Packet)
        {            if (Session == null || Session.GetHabbo() != null)
            {
                return;
            }

            if (Session.RC4Client == null && !Session.GetConnection().IsWebSocket)
            {
                return;
            }

            string SSOTicket = Packet.PopString();            int Timer = Packet.PopInt();

            //if (Timer <= 0) return;

            Session.TryAuthenticate(SSOTicket);
        }
    }
}
