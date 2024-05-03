namespace WibboEmulator.Games.Chats.Commands.Staff.Administration;

using WibboEmulator.Communication.Packets.Outgoing.Televisions;
using WibboEmulator.Core.Language;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal sealed class Youtube : IChatCommand
{
    public void Execute(GameClient session, Room room, RoomUser userRoom, string[] parameters)
    {
        if (parameters.Length < 3)
        {
            return;
        }

        var username = parameters[1];
        var url = parameters[2];

        if (string.IsNullOrEmpty(url) || (!url.Contains("?v=") && !url.Contains("youtu.be/")))
        {
            return;
        }

        var split = "";

        if (url.Contains("?v="))
        {
            split = url.Split(new string[] { "?v=" }, StringSplitOptions.None)[1];
        }
        else if (url.Contains("youtu.be/"))
        {
            split = url.Split(new string[] { "youtu.be/" }, StringSplitOptions.None)[1];
        }

        if (split.Length < 11)
        {
            return;
        }
        var videoId = split[..11];

        var roomUserByUserId = room.RoomUserManager.GetRoomUserByName(username);
        if (roomUserByUserId == null || roomUserByUserId.Client == null || roomUserByUserId.Client.User == null)
        {
            return;
        }

        if (session.Language != roomUserByUserId.Client.Language)
        {
            session.SendWhisper(string.Format(LanguageManager.TryGetValue("cmd.authorized.langue.user", session.Language), roomUserByUserId.Client.Language));
            return;
        }

        roomUserByUserId.Client.SendPacket(new YoutubeTvComposer(0, videoId));
    }
}
