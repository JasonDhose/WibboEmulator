using Butterfly.Communication.Packets.Outgoing;
using Butterfly.Game.GameClients;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class OnGuideSessionDetached : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            bool state = Packet.PopBoolean();

            GameClient requester = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUserID(Session.GetHabbo().GuideOtherUserId);

            if (!state)
            {
                ServerPacket finsession = new ServerPacket(ServerPacketHeader.OnGuideSessionDetached);
                Session.SendPacket(finsession);

                if (requester == null)
                {
                    return;
                }

                ServerPacket MessageNoGuide = new ServerPacket(ServerPacketHeader.OnGuideSessionError);
                MessageNoGuide.WriteInteger(1);
                requester.SendPacket(MessageNoGuide);
                return;
            }

            if (requester == null)
            {
                return;
            }

            ServerPacket message = new ServerPacket(ServerPacketHeader.OnGuideSessionStarted);
            message.WriteInteger(requester.GetHabbo().Id);
            message.WriteString(requester.GetHabbo().Username);
            message.WriteString(requester.GetHabbo().Look);
            message.WriteInteger(Session.GetHabbo().Id);
            message.WriteString(Session.GetHabbo().Username);
            message.WriteString(Session.GetHabbo().Look);

            requester.SendPacket(message);
            Session.SendPacket(message);
        }
    }
}
