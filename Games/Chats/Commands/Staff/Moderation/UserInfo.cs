namespace WibboEmulator.Games.Chats.Commands.Staff.Moderation;
using System.Text;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal sealed class UserInfo : IChatCommand
{
    public void Execute(GameClient session, Room room, RoomUser userRoom, string[] parameters)
    {
        if (parameters.Length != 2)
        {
            return;
        }

        var username = parameters[1];

        if (string.IsNullOrEmpty(username))
        {
            userRoom.SendWhisperChat(WibboEnvironment.GetLanguageManager().TryGetValue("input.userparammissing", session.Langue));
            return;
        }
        var clientByUsername = WibboEnvironment.GetGame().GetGameClientManager().GetClientByUsername(username);
        if (clientByUsername == null || clientByUsername.User == null)
        {
            userRoom.SendWhisperChat(WibboEnvironment.GetLanguageManager().TryGetValue("input.useroffline", session.Langue));
            return;
        }

        var user = clientByUsername.User;
        var stringBuilder = new StringBuilder();

        var totalPoints = user.WibboPoints + user.InventoryComponent.GetInventoryPoints();

        _ = stringBuilder.Append("- Information sur l'utilisateur [" + user.Username + "] -\r");
        _ = stringBuilder.Append("Nom: " + user.Username + "\r");
        _ = stringBuilder.Append("Id: " + user.Id + "\r");
        _ = stringBuilder.Append("Mission: " + user.Motto + "\r");
        _ = stringBuilder.Append("WibboPoints: " + totalPoints + "\r");
        _ = stringBuilder.Append("LimitCoins: " + user.LimitCoins + "\r");
        _ = stringBuilder.Append("Crédits: " + user.Credits + "\r");
        _ = stringBuilder.Append("Win-Win: " + user.AchievementPoints + "\r");
        _ = stringBuilder.Append("Premium: " + (user.Rank > 1 ? "Oui" : "Non") + "\r");
        _ = stringBuilder.Append("Mazo Score: " + user.MazoHighScore + "\r");
        _ = stringBuilder.Append("Respects: " + user.Respect + "\r");

        _ = stringBuilder.Append("Dans un appart: " + (user.InRoom ? "Oui" : "Non") + "\r");

        if (user.CurrentRoom != null && !user.SpectatorMode)
        {
            _ = stringBuilder.Append("\r - Information sur l'appart  [" + user.CurrentRoom.Id + "] - \r");
            _ = stringBuilder.Append("Propriétaire: " + user.CurrentRoom.RoomData.OwnerName + "\r");
            _ = stringBuilder.Append("Nom: " + user.CurrentRoom.RoomData.Name + "\r");
            _ = stringBuilder.Append("Utilisateurs: " + user.CurrentRoom.UserCount + "/" + user.CurrentRoom.RoomData.UsersMax + "\r");
        }

        if (session.User.HasPermission("god"))
        {
            _ = stringBuilder.Append("\r - Autre information - \r");
            _ = stringBuilder.Append("IP Web: " + clientByUsername.User.IP + "\r");
            _ = stringBuilder.Append("IP Emu: " + clientByUsername.Connection.GetIp() + "\r");
            _ = stringBuilder.Append("Langue: " + clientByUsername.Langue.ToString() + "\r");
        }

        session.SendNotification(stringBuilder.ToString());
    }
}
