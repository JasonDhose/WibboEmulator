﻿using WibboEmulator.Games.GameClients;

namespace WibboEmulator.Games.Items.Interactors
{
    public class InteractorChangeBackgrounds : FurniInteractor
    {
        public override void OnPlace(GameClient Session, Item Item)
        {
        }

        public override void OnRemove(GameClient Session, Item Item)
        {
        }

        public override void OnTrigger(GameClient Session, Item Item, int Request, bool UserHasRights, bool Reverse)
        {
            if (Session == null || Session.GetUser() == null || (Item == null || !UserHasRights) || Request != 0)
            {
                return;
            }

            if (Item.ExtraData.StartsWith("on"))
            {
                Item.ExtraData = Item.ExtraData.Replace("on", "off");
            }
            else if (Item.ExtraData.StartsWith("off"))
            {
                Item.ExtraData = Item.ExtraData.Replace("off", "on");
            }

            Item.UpdateState();
        }

        public override void OnTick(Item item)
        {
        }
    }
}
