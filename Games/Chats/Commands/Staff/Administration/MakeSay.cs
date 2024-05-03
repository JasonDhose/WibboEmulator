namespace WibboEmulator.Games.Chats.Commands.Staff.Administration;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal sealed class MakeSay : IChatCommand
{
    public void Execute(GameClient session, Room room, RoomUser userRoom, string[] parameters)
    {
        if (parameters.Length < 3)
        {
            return;
        }

        var username = parameters[1];
        var message = CommandManager.MergeParams(parameters, 2);

        var roomUserByUserId = session.User.Room.RoomUserManager.GetRoomUserByName(username);
        if (roomUserByUserId == null)
        {
            return;
        }

        roomUserByUserId.OnChat(message, 0, false);
    }
}
