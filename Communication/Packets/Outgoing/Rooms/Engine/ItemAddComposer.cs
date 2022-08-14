using WibboEmulator.Game.Items;

namespace WibboEmulator.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class ItemAddComposer : ServerPacket
    {
        public ItemAddComposer(Item Item, string Username, int UserID)
            : base(ServerPacketHeader.ITEM_WALL_ADD)
        {
            this.WriteString(Item.Id.ToString());
            this.WriteInteger(Item.GetBaseItem().SpriteId);
            this.WriteString(Item.WallCoord != null ? Item.WallCoord : string.Empty);

            ItemBehaviourUtility.GenerateWallExtradata(Item, this);

            this.WriteInteger(-1);
            this.WriteInteger((Item.GetBaseItem().Modes > 1) ? 1 : 0);
            this.WriteInteger(UserID);
            this.WriteString(Username);
        }
    }
}
