﻿using Butterfly.Game.Clients;
using Butterfly.Game.Rooms;

namespace Butterfly.Game.Items.Interactors
{
    public class InteractorOneWayGate : FurniInteractor
    {
        public override void OnPlace(Client Session, Item Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser == 0)
            {
                return;
            }

            RoomUser roomUserByHabbo = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabboId(Item.InteractingUser);
            if (roomUserByHabbo != null)
            {
                roomUserByHabbo.UnlockWalking();
            }

            Item.InteractingUser = 0;
        }

        public override void OnRemove(Client Session, Item Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser == 0)
            {
                return;
            }

            RoomUser roomUserByHabbo = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabboId(Item.InteractingUser);
            if (roomUserByHabbo != null)
            {
                roomUserByHabbo.UnlockWalking();
            }

            Item.InteractingUser = 0;
        }

        public override void OnTrigger(Client Session, Item Item, int Request, bool UserHasRights, bool Reverse)
        {
            if (Session == null || Session.GetHabbo() == null || Item == null || Item.GetRoom() == null)
            {
                return;
            }

            RoomUser roomUser = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabboId(Session.GetHabbo().Id);
            if (roomUser == null)
            {
                return;
            }

            if (roomUser.Coordinate != Item.SquareInFront && roomUser.CanWalk)
            {
                roomUser.MoveTo(Item.SquareInFront);
            }
            else
            {
                if (!roomUser.CanWalk)
                {
                    return;
                }

                if (!Item.GetRoom().GetGameMap().CanWalk(Item.SquareBehind.X, Item.SquareBehind.Y, roomUser.AllowOverride))
                {
                    return;
                }

                RoomUser roomUserTarget = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabboId(Item.InteractingUser);
                if (roomUserTarget != null)
                {
                    return;
                }

                Item.InteractingUser = 0;

                Item.InteractingUser = roomUser.HabboId;
                roomUser.CanWalk = false;

                roomUser.AllowOverride = true;
                roomUser.MoveTo(Item.SquareBehind);

                Item.ExtraData = "1";
                Item.UpdateState(false, true);

                Item.ReqUpdate(1);
            }
        }

        public override void OnTick(Item item)
        {
            RoomUser roomUserTarget = null;
            if (item.InteractingUser > 0)
            {
                roomUserTarget = item.GetRoom().GetRoomUserManager().GetRoomUserByHabboId(item.InteractingUser);
            }

            if (roomUserTarget == null)
            {
                item.InteractingUser = 0;
                return;
            }

            if (roomUserTarget.Coordinate == item.SquareBehind || !Gamemap.TilesTouching(item.X, item.Y, roomUserTarget.X, roomUserTarget.Y))
            {
                roomUserTarget.UnlockWalking();
                item.ExtraData = "0";
                item.InteractingUser = 0;
                item.UpdateState(false, true);
            }
            else
            {
                roomUserTarget.CanWalk = false;
                roomUserTarget.AllowOverride = true;
                roomUserTarget.MoveTo(item.SquareBehind);

                item.UpdateCounter = 1;
            }
        }
    }
}
