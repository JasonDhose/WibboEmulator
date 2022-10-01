using WibboEmulator.Communication.Packets.Outgoing.Help;
using WibboEmulator.Games.GameClients;

namespace WibboEmulator.Communication.Packets.Incoming.Guide
{
    internal class GuideToolMessageNewEvent : IPacketEvent
    {
        public double Delay => 250;

        public void Parse(GameClient Session, ClientPacket Packet)
        {
            string message = Packet.PopString();

            GameClient requester = WibboEnvironment.GetGame().GetClientManager().GetClientByUserID(Session.GetUser().GuideOtherUserId);
            if (requester == null)
            {
                return;
            }

            if (Session.Antipub(message, "<GUIDEMESSAGE>"))
            {
                return;
            }

            requester.SendPacket(new OnGuideSessionMsgComposer(message, Session.GetUser().Id));
            Session.SendPacket(new OnGuideSessionMsgComposer(message, Session.GetUser().Id));
        }
    }
}