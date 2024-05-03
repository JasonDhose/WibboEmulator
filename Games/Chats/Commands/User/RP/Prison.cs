namespace WibboEmulator.Games.Chats.Commands.User.RP;

using WibboEmulator.Core.Language;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal sealed class Prison : IChatCommand
{
    public void Execute(GameClient session, Room room, RoomUser userRoom, string[] parameters)
    {
        if (parameters.Length != 2)
        {
            return;
        }

        if (!room.IsRoleplay)
        {
            return;
        }

        if (!room.RoomRoleplay.Pvp)
        {
            return;
        }

        var rp = userRoom.Roleplayer;
        if (rp == null)
        {
            return;
        }

        if (rp.Dead || rp.SendPrison)
        {
            return;
        }

        var targetRoomUser = room.RoomUserManager.GetRoomUserByName(parameters[1].ToString());

        if (targetRoomUser == null)
        {
            return;
        }

        var rpTwo = targetRoomUser.Roleplayer;
        if (rpTwo == null)
        {
            return;
        }

        if (targetRoomUser.Client.User.Id == session.User.Id)
        {
            return;
        }

        if (rpTwo.Dead || rpTwo.SendPrison)
        {
            return;
        }

        if (rpTwo.SendPrison)
        {
            return;
        }

        if (Math.Floor((double)(rpTwo.Health / (double)rpTwo.HealthMax) * 100) > 75)
        {
            userRoom.OnChat(string.Format(LanguageManager.TryGetValue("cmd.prison.chat.fail", session.Language), targetRoomUser.Username));
            session.SendWhisper(LanguageManager.TryGetValue("rp.prisonnotallowed", session.Language));
            return;
        }

        if (!(Math.Abs(targetRoomUser.X - userRoom.X) >= 2 || Math.Abs(targetRoomUser.Y - userRoom.Y) >= 2))
        {
            userRoom.OnChat(string.Format(LanguageManager.TryGetValue("cmd.prison.chat.success", session.Language), targetRoomUser.Username));

            targetRoomUser.ApplyEffect(729, true);
            targetRoomUser.RotBody = 2;
            targetRoomUser.RotHead = 2;
            targetRoomUser.SetStatus("sit", "0.5");
            targetRoomUser.Freeze = true;
            targetRoomUser.FreezeEndCounter = 0;
            targetRoomUser.IsSit = true;
            targetRoomUser.UpdateNeeded = true;

            rpTwo.SendPrison = true;
            rpTwo.PrisonTimer = 10 * 2;
        }

        if (userRoom.FreezeEndCounter <= 2)
        {
            userRoom.Freeze = true;
            userRoom.FreezeEndCounter = 2;
        }
    }
}
