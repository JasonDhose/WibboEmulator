﻿using Butterfly.Game.Clients;
using Butterfly.Game.Rooms;
using Butterfly.Game.Rooms.Pathfinding;

namespace Butterfly.Game.Items.Interactors
{
    public class InteractorVendorEnable : FurniInteractor
    {
        public override void OnPlace(Client Session, Item Item)
        {
            Item.ExtraData = "0";
            if (Item.InteractingUser <= 0)
            {
                return;
            }

            Item.InteractingUser = 0;
        }

        public override void OnRemove(Client Session, Item Item)
        {
            Item.ExtraData = "0";
            if (Item.InteractingUser <= 0)
            {
                return;
            }

            Item.InteractingUser = 0;
        }

        public override void OnTrigger(Client Session, Item Item, int Request, bool UserHasRights, bool Reverse)
        {
            if (!(Item.ExtraData != "1") || Item.GetBaseItem().VendingIds.Count < 1 || (Item.InteractingUser != 0 || Session == null))
            {
                return;
            }

            RoomUser roomUserByUserId = Item.GetRoom().GetRoomUserManager().GetRoomUserByUserId(Session.GetUser().Id);
            if (roomUserByUserId == null)
            {
                return;
            }

            if (!Gamemap.TilesTouching(roomUserByUserId.X, roomUserByUserId.Y, Item.X, Item.Y))
            {
                roomUserByUserId.MoveTo(Item.SquareInFront);
            }
            else
            {
                Item.InteractingUser = Session.GetUser().Id;
                roomUserByUserId.SetRot(Rotation.Calculate(roomUserByUserId.X, roomUserByUserId.Y, Item.X, Item.Y), false);
                Item.ReqUpdate(2);
                Item.ExtraData = "1";
                Item.UpdateState(false, true);
            }
        }

        public override void OnTick(Item item)
        {
            if (!(item.ExtraData == "1"))
            {
                return;
            }

            RoomUser roomUserByUserId = item.GetRoom().GetRoomUserManager().GetRoomUserByUserId(item.InteractingUser);
            if (roomUserByUserId != null)
            {
                int vendingId = item.GetBaseItem().VendingIds[ButterflyEnvironment.GetRandomNumber(0, item.GetBaseItem().VendingIds.Count - 1)];
                roomUserByUserId.ApplyEffect(vendingId);
            }

            item.InteractingUser = 0;
            item.ExtraData = "0";
            item.UpdateState(false, true);
        }
    }
}
