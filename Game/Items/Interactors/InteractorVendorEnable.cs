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

        public override void OnTrigger(Client Session, Item Item, int Request, bool UserHasRights)
        {
            if (!(Item.ExtraData != "1") || Item.GetBaseItem().VendingIds.Count < 1 || (Item.InteractingUser != 0 || Session == null))
            {
                return;
            }

            RoomUser roomUserByHabbo = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabboId(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
            {
                return;
            }

            if (!Gamemap.TilesTouching(roomUserByHabbo.X, roomUserByHabbo.Y, Item.X, Item.Y))
            {
                roomUserByHabbo.MoveTo(Item.SquareInFront);
            }
            else
            {
                Item.InteractingUser = Session.GetHabbo().Id;
                roomUserByHabbo.SetRot(Rotation.Calculate(roomUserByHabbo.X, roomUserByHabbo.Y, Item.X, Item.Y), false);
                Item.ReqUpdate(2);
                Item.ExtraData = "1";
                Item.UpdateState(false, true);
            }
        }
    }
}
