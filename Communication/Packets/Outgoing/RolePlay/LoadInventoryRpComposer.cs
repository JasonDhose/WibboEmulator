namespace WibboEmulator.Communication.Packets.Outgoing.RolePlay;
using System.Collections.Concurrent;
using WibboEmulator.Games.Roleplays.Item;
using WibboEmulator.Games.Roleplays.Player;

internal sealed class LoadInventoryRpComposer : ServerPacket
{
    public LoadInventoryRpComposer(ConcurrentDictionary<int, RolePlayInventoryItem> items)
      : base(ServerPacketHeader.LOAD_INVENTORY_RP)
    {
        this.WriteInteger(items.Count);

        foreach (var item in items.Values)
        {
            var rpItem = RPItemManager.GetItem(item.ItemId);

            this.WriteInteger(item.ItemId);
            this.WriteString(rpItem.Name);
            this.WriteString(rpItem.Desc);
            this.WriteInteger(item.Count);
            this.WriteInteger((int)rpItem.Category);
            this.WriteInteger(rpItem.UseType);
        }
    }
}
