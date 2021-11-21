﻿using Butterfly.Game.Clients;

namespace Butterfly.Game.Items.Interactors
{
    public class WiredInteractor : FurniInteractor
    {
        public override void OnPlace(Client Session, Item Item)
        {
        }

        public override void OnRemove(Client Session, Item Item)
        {
        }

        public override void OnTrigger(Client Session, Item Item, int Request, bool UserHasRights)
        {
            if (Session == null || Item == null || !UserHasRights)
            {
                return;
            }

            if (Item.WiredHandler != null)
            {
                Item.WiredHandler.OnTrigger(Session, Item.GetBaseItem().SpriteId);
            }
        }
    }
}
