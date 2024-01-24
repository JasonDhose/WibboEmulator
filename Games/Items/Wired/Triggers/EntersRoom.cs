namespace WibboEmulator.Games.Items.Wired.Triggers;
using System.Data;
using WibboEmulator.Games.Items.Wired.Bases;
using WibboEmulator.Games.Items.Wired.Interfaces;
using WibboEmulator.Games.Rooms;

public class EntersRoom : WiredTriggerBase, IWired
{
    public EntersRoom(Item item, Room room) : base(item, room, (int)WiredTriggerType.AVATAR_ENTERS_ROOM) => this.RoomInstance.RoomUserManager.OnUserEnter += this.OnUserEnter;

    private void OnUserEnter(object sender, EventArgs e)
    {
        var user = (RoomUser)sender;
        if (user == null)
        {
            return;
        }

        if ((user.IsBot || string.IsNullOrEmpty(this.StringParam) || string.IsNullOrEmpty(this.StringParam) || !(user.GetUsername() == this.StringParam)) && !string.IsNullOrEmpty(this.StringParam))
        {
            return;
        }

        this.RoomInstance.WiredHandler.ExecutePile(this.ItemInstance.Coordinate, user, null);
    }

    public override void Dispose()
    {
        base.Dispose();

        this.RoomInstance.RoomUserManager.OnUserEnter -= this.OnUserEnter;
    }

    public void SaveToDatabase(IDbConnection dbClient) => WiredUtillity.SaveInDatabase(dbClient, this.Id, string.Empty, this.StringParam, false, null);

    public void LoadFromDatabase(string wiredTriggerData, string wiredTriggerData2, string wiredTriggersItem, bool wiredAllUserTriggerable, int wiredDelay) => this.StringParam = wiredTriggerData;
}
