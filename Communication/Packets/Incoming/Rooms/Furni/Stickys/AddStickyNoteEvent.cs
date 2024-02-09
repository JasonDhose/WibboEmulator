namespace WibboEmulator.Communication.Packets.Incoming.Rooms.Furni.Stickys;

using WibboEmulator.Core;
using WibboEmulator.Database.Daos.Item;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Items;

internal sealed class AddStickyNoteEvent : IPacketEvent
{
    public double Delay => 250;

    public void Parse(GameClient session, ClientPacket packet)
    {
        if (!WibboEnvironment.GetGame().GetRoomManager().TryGetRoom(session.User.CurrentRoomId, out var room))
        {
            return;
        }

        if (!room.CheckRights(session))
        {
            return;
        }

        var id = packet.PopInt();
        var str = packet.PopString();

        var userItem = session.User.InventoryComponent.GetItem(id);
        if (userItem == null || !userItem.IsWallItem || userItem.Data.InteractionType != InteractionType.POSTIT)
        {
            return;
        }

        if (room == null)
        {
            return;
        }

        var wallCoord = WallPositionCheck(":" + str.Split(':')[1]);
        var roomItem = new Item(userItem.Id, room.Id, userItem.BaseItem, userItem.ExtraData, userItem.Limited, userItem.LimitedStack, 0, 0, 0.0, 0, wallCoord, room);
        if (!room.RoomItemHandling.SetWallItem(session, roomItem))
        {
            return;
        }

        using (var dbClient = WibboEnvironment.GetDatabaseManager().Connection())
        {
            ItemDao.UpdateRoomIdAndUserId(dbClient, id, room.Id, room.RoomData.OwnerId);
        }

        session.User.InventoryComponent.RemoveItem(id);
    }

    private static string WallPositionCheck(string wallPosition)
    {
        //:w=3,2 l=9,63 l
        try
        {
            if (wallPosition.Contains(Convert.ToChar(13)) || wallPosition.Contains(Convert.ToChar(9)))
            {
                return ":w=0,0 l=0,0 l";
            }

            var posD = wallPosition.Split(' ');
            if (posD[2] is not "l" and not "r")
            {
                return ":w=0,0 l=0,0 l";
            }

            var widD = posD[0][3..].Split(',');
            var widthX = int.Parse(widD[0]);
            var widthY = int.Parse(widD[1]);
            //if (widthX < 0 || widthY < 0 || widthX > 200 || widthY > 200)
            //return ":w=0,0 l=0,0 l";

            var lenD = posD[1][2..].Split(',');
            var lengthX = int.Parse(lenD[0]);
            var lengthY = int.Parse(lenD[1]);
            //if (lengthX < 0 || lengthY < 0 || lengthX > 200 || lengthY > 200)
            //return ":w=0,0 l=0,0 l";
            return ":w=" + widthX + "," + widthY + " " + "l=" + lengthX + "," + lengthY + " " + posD[2];
        }
        catch (Exception ex)
        {
            ExceptionLogger.LogException(ex.ToString());
            return ":w=0,0 l=0,0 l";
        }
    }

}
