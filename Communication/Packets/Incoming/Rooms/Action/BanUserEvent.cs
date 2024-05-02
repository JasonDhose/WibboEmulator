namespace WibboEmulator.Communication.Packets.Incoming.Rooms.Action;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal sealed class BanUserEvent : IPacketEvent
{
    public double Delay => 250;

    public void Parse(GameClient session, ClientPacket packet)
    {
        if (session.User == null)
        {
            return;
        }

        if (!RoomManager.TryGetRoom(session.User.RoomId, out var room))
        {
            return;
        }

        if ((room.RoomData.BanFuse != 1 || !room.CheckRights(session)) && !room.CheckRights(session, true))
        {
            return;
        }

        var pId = packet.PopInt();

        _ = packet.PopInt();
        var str = packet.PopString();

        var roomUserByUserId = room.RoomUserManager.GetRoomUserByUserId(pId);
        int time;
        if (str.Equals("RWUAM_BAN_USER_HOUR"))
        {
            time = 3600;
        }
        else if (str.Equals("RWUAM_BAN_USER_DAY"))
        {
            time = 86400;
        }
        else
        {
            if (!str.Equals("RWUAM_BAN_USER_PERM"))
            {
                return;
            }

            time = 429496729;
        }
        if (roomUserByUserId == null || roomUserByUserId.IsBot || room.CheckRights(roomUserByUserId.Client, true) || roomUserByUserId.Client.User.HasPermission("kick") || roomUserByUserId.Client.User.HasPermission("no_kick"))
        {
            return;
        }

        room.AddBan(pId, time);
        room.RoomUserManager.RemoveUserFromRoom(roomUserByUserId.Client, true, true);
    }
}
