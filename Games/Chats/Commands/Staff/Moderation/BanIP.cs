namespace WibboEmulator.Games.Chats.Commands.Staff.Moderation;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal sealed class BanIP : IChatCommand
{
    public void Execute(GameClient session, Room room, RoomUser userRoom, string[] parameters)
    {
        if (parameters.Length < 2)
        {
            return;
        }

        var targetUser = WibboEnvironment.GetGame().GetGameClientManager().GetClientByUsername(parameters[1]);
        if (targetUser == null || targetUser.User == null)
        {
            userRoom.SendWhisperChat(WibboEnvironment.GetLanguageManager().TryGetValue("input.usernotfound", session.Langue));
            return;
        }

        if (targetUser.User.Rank >= session.User.Rank)
        {
            userRoom.SendWhisperChat(WibboEnvironment.GetLanguageManager().TryGetValue("action.notallowed", session.Langue));
            return;
        }

        var reason = "";
        if (parameters.Length > 2)
        {
            reason = CommandManager.MergeParams(parameters, 2);
        }

        var securityBan = targetUser.User.Rank > 5 && session.User.Rank < 12;

        session.SendWhisper("Tu as banIP " + targetUser.User.Username + " pour " + reason + "!");

        WibboEnvironment.GetGame().GetGameClientManager().BanUser(targetUser, session.User.Username, -1, reason, true, false);
        _ = session.User.CheckChatMessage(reason, "<CMD>", room.Id);

        if (securityBan)
        {
            WibboEnvironment.GetGame().GetGameClientManager().BanUser(session, "Robot", -1, "Votre compte à été banni par sécurité", false, false);
        }
    }
}
