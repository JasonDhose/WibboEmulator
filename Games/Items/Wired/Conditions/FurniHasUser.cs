namespace WibboEmulator.Games.Items.Wired.Conditions;
using System.Data;
using WibboEmulator.Games.Items.Wired.Bases;
using WibboEmulator.Games.Items.Wired.Interfaces;
using WibboEmulator.Games.Rooms;

public class FurniHasUser(Item item, Room room) : WiredConditionBase(item, room, (int)WiredConditionType.FURNIS_HAVE_AVATARS), IWiredCondition, IWired
{
    public bool AllowsExecution(RoomUser user, Item item)
    {
        foreach (var itemList in this.Items.ToList())
        {
            foreach (var coord in itemList.GetAffectedTiles)
            {
                if (this.Room.GameMap.GetRoomUsers(coord).Count == 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void SaveToDatabase(IDbConnection dbClient) => WiredUtillity.SaveInDatabase(dbClient, this.Id, string.Empty, string.Empty, false, this.Items);

    public void LoadFromDatabase(string wiredTriggerData, string wiredTriggerData2, string wiredTriggersItem, bool wiredAllUserTriggerable, int wiredDelay) => this.LoadStuffIds(wiredTriggersItem);
}
