namespace WibboEmulator.Communication.Packets.Incoming.Rooms.Engine;
using WibboEmulator.Games.GameClients;

internal sealed class MoveAvatarKeyboardEvent : IPacketEvent
{
    public double Delay => 0;

    public void Parse(GameClient Session, ClientPacket packet)
    {
        var targetX = packet.PopInt();
        var targetY = packet.PopInt();

        if (targetX is > 1 or < (-1))
        {
            targetX = 0;
        }

        if (targetY is > 1 or < (-1))
        {
            targetY = 0;
        }

        if (Session == null || Session.User == null)
        {
            return;
        }

        var currentRoom = Session.User.Room;
        if (currentRoom == null)
        {
            return;
        }

        var user = currentRoom.RoomUserManager.GetRoomUserByUserId(Session.User.Id);

        if (user == null || (!user.CanWalk && !user.TeleportEnabled) || !user.AllowArrowMove || !user.AllowMoveTo)
        {
            return;
        }

        user.Unidle();

        user.IsWalking = true;

        if (user.ReverseWalk)
        {
            user.GoalX = user.SetX + (-targetX * 1000);
            user.GoalY = user.SetY + (-targetY * 1000);
        }
        else
        {
            user.GoalX = user.SetX + (targetX * 1000);
            user.GoalY = user.SetY + (targetY * 1000);
        }

    }
}
