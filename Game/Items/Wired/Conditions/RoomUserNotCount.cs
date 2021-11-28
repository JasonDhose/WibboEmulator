﻿using Butterfly.Database.Interfaces;
using Butterfly.Game.Rooms;
using Butterfly.Game.Items.Wired.Interfaces;
using System.Data;

namespace Butterfly.Game.Items.Wired.Conditions
{
    public class RoomUserNotCount : WiredConditionBase, IWiredCondition, IWired
    {
        public RoomUserNotCount(Item item, Room room) : base(item, room, (int)WiredConditionType.NOT_USER_COUNT_IN)
        {
        }

        public override void LoadItems(bool inDatabase = false)
        {
            base.LoadItems(inDatabase);

            if (inDatabase)
                return;

            this.IntParams.Add(0);
            this.IntParams.Add(0);
        }

        public bool AllowsExecution(RoomUser user, Item TriggerItem)
        {
            int minUsers = ((this.IntParams.Count > 0) ? this.IntParams[0] : 0);
            int maxUsers = ((this.IntParams.Count > 1) ? this.IntParams[1] : 0);

            if (this.RoomInstance.UserCount > minUsers)
            {
                return false;
            }

            if (this.RoomInstance.UserCount < maxUsers)
            {
                return false;
            }

            return true;
        }

        public void SaveToDatabase(IQueryAdapter dbClient)
        {
            int minUsers = ((this.IntParams.Count > 0) ? this.IntParams[0] : 0);
            int maxUsers = ((this.IntParams.Count > 1) ? this.IntParams[1] : 0);

            WiredUtillity.SaveTriggerItem(dbClient, this.Id, string.Empty, minUsers + ":" + maxUsers, false, null);
        }

        public void LoadFromDatabase(DataRow row)
        {
            string triggerData = row["trigger_data"].ToString();
            if (!triggerData.Contains(":"))
            {
                return;
            }

            string countMin = triggerData.Split(':')[0];
            string countMax = triggerData.Split(':')[1];

            if (int.TryParse(countMin, out int minUsers))
                this.IntParams.Add(minUsers);

            if (int.TryParse(countMax, out int maxUsers))
                this.IntParams.Add(maxUsers);
        }
    }
}
