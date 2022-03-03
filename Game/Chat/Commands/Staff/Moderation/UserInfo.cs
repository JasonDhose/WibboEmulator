using Butterfly.Game.Clients;
using Butterfly.Game.Users;
using System.Text;
using Butterfly.Game.Rooms;

namespace Butterfly.Game.Chat.Commands.Cmd
{
    internal class UserInfo : IChatCommand
    {
        public void Execute(Client Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            if (Params.Length != 2)
            {
                return;
            }

            string username = Params[1];

            if (string.IsNullOrEmpty(username))
            {
                Session.SendNotification(ButterflyEnvironment.GetLanguageManager().TryGetValue("input.userparammissing", Session.Langue));
                return;
            }
            Client clientByUsername = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            if (clientByUsername == null || clientByUsername.GetUser() == null)
            {
                Session.SendNotification(ButterflyEnvironment.GetLanguageManager().TryGetValue("input.useroffline", Session.Langue));
                return;
            }

            User user = clientByUsername.GetUser();
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Nom: " + user.Username + "\r");
            stringBuilder.Append("Id: " + user.Id + "\r");
            stringBuilder.Append("Mission: " + user.Motto + "\r");
            stringBuilder.Append("WibboPoints: " + user.WibboPoints + "\r");
            stringBuilder.Append("Crédits: " + user.Credits + "\r");
            stringBuilder.Append("Win-Win: " + user.AchievementPoints + "\r");
            stringBuilder.Append("Premium: " + ((user.Rank > 1) ? "Oui" : "Non") + "\r");
            stringBuilder.Append("Mazo Score: " + user.MazoHighScore + "\r");
            stringBuilder.Append("Respects: " + user.Respect + "\r");
            stringBuilder.Append("Dans un appart: " + ((user.InRoom) ? "Oui" : "Non") + "\r");

            if (user.CurrentRoom != null && !user.SpectatorMode)
            {
                stringBuilder.Append("\r - Information sur l'appart  [" + user.CurrentRoom.Id + "] - \r");
                stringBuilder.Append("Propriétaire: " + user.CurrentRoom.RoomData.OwnerName + "\r");
                stringBuilder.Append("Nom: " + user.CurrentRoom.RoomData.Name + "\r");
                stringBuilder.Append("Utilisateurs: " + user.CurrentRoom.UserCount + "/" + user.CurrentRoom.RoomData.UsersMax + "\r");
            }

            if (Session.GetUser().HasFuse("fuse_infouser"))
            {
                stringBuilder.Append("\r - Autre information - \r");
                stringBuilder.Append("MachineId: " + clientByUsername.MachineId + "\r");
                stringBuilder.Append("IP Web: " + clientByUsername.GetUser().IP + "\r");
                stringBuilder.Append("IP Emu: " + clientByUsername.GetConnection().GetIp() + "\r");
                stringBuilder.Append("Langue: " + clientByUsername.Langue.ToString() + "\r");
                stringBuilder.Append("Client: " + ((clientByUsername.GetConnection().IsWebSocket) ? "Nitro" : "Flash") + "\r");

                WebClients.WebClient ClientWeb = ButterflyEnvironment.GetGame().GetClientWebManager().GetClientByUserID(user.Id);
                if (ClientWeb != null)
                {
                    stringBuilder.Append("WebSocket: En ligne" + "\r");
                    if (Session.GetUser().Rank > 12)
                    {
                        stringBuilder.Append("WebSocket Ip: " + ClientWeb.GetConnection().GetIp() + "\r");
                        stringBuilder.Append("Langue Web: " + ClientWeb.Langue.ToString() + "\r");
                    }
                }
                else
                {
                    stringBuilder.Append("WebSocket: Hors ligne" + "\r");
                }
            }

            Session.SendNotification(stringBuilder.ToString());

        }
    }
}