using Butterfly.Communication.Packets.Outgoing.Users;
using Butterfly.Game.Clients;
using System.Linq;

namespace Butterfly.Game.Rooms.Chat.Commands.Cmd
{
    internal class MassBadge : IChatCommand
    {
        public void Execute(Client Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            string Badge = Params[1];

            if (string.IsNullOrEmpty(Badge))
            {
                return;
            }

            foreach (Client Client in ButterflyEnvironment.GetGame().GetClientManager().GetClients.ToList())
            {
                if (Client.GetHabbo() != null)
                {
                    Client.GetHabbo().GetBadgeComponent().GiveBadge(Badge, true);
                    Client.SendPacket(new ReceiveBadgeComposer(Badge));
                    Client.SendNotification("Vous venez de recevoir le badge : " + Badge + " !");
                }
            }

        }
    }
}
