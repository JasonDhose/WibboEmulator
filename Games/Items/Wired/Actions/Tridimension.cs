namespace WibboEmulator.Games.Items.Wired.Actions;
using System.Data;
using WibboEmulator.Games.Items.Wired.Bases;
using WibboEmulator.Games.Items.Wired.Interfaces;
using WibboEmulator.Games.Rooms;

public class Tridimension : WiredActionBase, IWiredEffect, IWired
{
    public Tridimension(Item item, Room room) : base(item, room, (int)WiredActionType.TRI_DIMENSION) => this.StringParam = "0;0;0.0;0;0";

    public override bool OnCycle(RoomUser user, Item item)
    {
        var disableAnimation = this.RoomInstance.WiredHandler.DisableAnimate(this.ItemInstance.Coordinate);

        var itemList = this.Items.ToList();
        if (itemList.Count >= 1)
        {
            foreach (var roomItem in this.Items.ToList())
            {
                this.HandleItem(roomItem, disableAnimation);
            }
        }
        else if (item != null)
        {
            this.HandleItem(item, disableAnimation);
        }

        return false;
    }

    private void HandleItem(Item roomItem, bool disableAnimation)
    {
        if (this.RoomInstance.RoomItemHandling.GetItem(roomItem.Id) == null)
        {
            return;
        }

        if (this.StringParam.Contains(';') == false)
        {
            return;
        }

        var parts = this.StringParam.Split(';');

        var x = 0;
        var y = 0;
        var z = 0.0;
        var rot = 0;
        var state = 0;

        if (parts.Length >= 3)
        {
            _ = int.TryParse(parts[0], out x);
            _ = int.TryParse(parts[1], out y);
            _ = double.TryParse(parts[2], out z);
        }

        if (parts.Length == 5)
        {
            _ = int.TryParse(parts[3], out rot);
            _ = int.TryParse(parts[4], out state);
        }

        var newX = roomItem.X + x;
        var newY = roomItem.Y + y;
        var newZ = roomItem.Z + z;

        if (newX > this.RoomInstance.GameMap.Model.MapSizeX)
        {
            newX = this.RoomInstance.GameMap.Model.MapSizeX - 1;
        }

        if (newY > this.RoomInstance.GameMap.Model.MapSizeY)
        {
            newY = this.RoomInstance.GameMap.Model.MapSizeY - 1;
        }

        if (newX < 0)
        {
            newX = 0;
        }

        if (newY < 0)
        {
            newY = 0;
        }

        if (newZ > 1000)
        {
            newZ = 1000;
        }

        if (newZ < -1000)
        {
            newZ = -1000;
        }

        if (newX != roomItem.X || newY != roomItem.Y || newZ != roomItem.Z)
        {
            if (this.RoomInstance.GameMap.ValidTile(newX, newY, newZ))
            {
                this.RoomInstance.RoomItemHandling.PositionReset(roomItem, newX, newY, newZ, disableAnimation);
            }
        }

        var needUpdate = false;

        var newRot = (roomItem.Rotation + rot) % 8;

        newRot = newRot < 0 ? newRot + 8 : newRot;

        if (roomItem.Rotation != newRot)
        {
            roomItem.Rotation = newRot;
            needUpdate = true;
        }

        if (roomItem.GetBaseItem().Modes > 1)
        {
            if (int.TryParse(roomItem.ExtraData, out var stateItem))
            {
                var newState = (stateItem + state) % roomItem.GetBaseItem().Modes;
                newState = newState < 0 ? newState + roomItem.GetBaseItem().Modes : newState;

                if (newState != stateItem)
                {
                    roomItem.ExtraData = newState.ToString();
                    needUpdate = true;
                }
            }
        }

        if (needUpdate)
        {
            roomItem.UpdateState();
        }

        return;
    }

    public void SaveToDatabase(IDbConnection dbClient) => WiredUtillity.SaveTriggerItem(dbClient, this.Id, string.Empty, this.StringParam, false, this.Items, this.Delay);

    public void LoadFromDatabase(string wiredTriggerData, string wiredTriggerData2, string wiredTriggersItem, bool wiredAllUserTriggerable, int wiredDelay)
    {
        this.Delay = wiredDelay;

        this.StringParam = wiredTriggerData;

        this.LoadStuffIds(wiredTriggersItem);
    }
}
