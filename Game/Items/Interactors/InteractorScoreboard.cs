﻿using Butterfly.Game.Clients;

namespace Butterfly.Game.Items.Interactors
{
    public class InteractorScoreboard : FurniInteractor
    {
        public override void OnPlace(Client Session, Item Item)
        {
        }

        public override void OnRemove(Client Session, Item Item)
        {
        }

        public override void OnTrigger(Client Session, Item Item, int Request, bool UserHasRights)
        {
            if (!UserHasRights)
            {
                return;
            }

            int num = 0;
            if (!string.IsNullOrEmpty(Item.ExtraData))
            {
                try
                {
                    num = int.Parse(Item.ExtraData);
                }
                catch
                {
                }
            }

            if (Request == 1)
            {
                if (num > 0)
                {
                    num -= 1;
                }
                else
                {
                    num = 99;
                }
            }
            else if (Request == 2)
            {
                if (num < 99)
                {
                    num += 1;
                }
                else
                {
                    num = 0;
                }
            }
            else if (Request == 3)
            {
                num = 0;
            }

            Item.ExtraData = num.ToString();
            Item.UpdateState();
        }
    }
}
