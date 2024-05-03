namespace WibboEmulator.Games.Items.Wired.Bases;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Wireds;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Items.Wired.Interfaces;
using WibboEmulator.Games.Rooms;
using WibboEmulator.Games.Rooms.Wired;

public class WiredActionBase : WiredBase, IWiredCycleable
{
    public int DelayCycle => this.Delay;
    public bool IsTeleport => false;

    public virtual void Handle(RoomUser user, Item item)
    {
        if (this.DelayCycle > 0)
        {
            this.Room.WiredHandler.RequestCycle(new WiredCycle(this, user, item));
        }
        else
        {
            _ = this.OnCycle(user, item);
        }
    }

    public virtual bool OnCycle(RoomUser user, Item item) => false;

    internal WiredActionBase(Item item, Room room, int type) : base(item, room, type)
    {
    }

    public override void OnTrigger(GameClient session) => session.SendPacket(new WiredFurniActionComposer(this.StuffTypeSelectionEnabled, this.FurniLimit, this.StuffIds, this.StuffTypeId, this.Id,
            this.StringParam, this.IntParams, this.StuffTypeSelectionCode, this.Type, this.Delay, this.Conflicting));
}
