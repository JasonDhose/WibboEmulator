using Butterfly.Communication.Packets.Outgoing.LandingView;
using Butterfly.Game.Clients;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class RefreshCampaignEvent : IPacketEvent
    {
        public void Parse(Client Session, ClientPacket Packet)
        {
            try
            {
                string parseCampaings = Packet.PopString();
                if (parseCampaings.Contains("gamesmaker"))
                {
                    return;
                }

                string campaingName = "";
                string[] parser = parseCampaings.Split(';');

                for (int i = 0; i < parser.Length; i++)
                {
                    if (string.IsNullOrEmpty(parser[i]) || parser[i].EndsWith(","))
                    {
                        continue;
                    }

                    string[] data = parser[i].Split(',');
                    campaingName = data[1];
                }
                Session.SendPacket(new CampaignComposer(parseCampaings, campaingName));
            }
            catch { }
        }
    }
}
