﻿using Butterfly.Database.Interfaces;
using Butterfly.Game.Rooms;
using Butterfly.Game.Items.Wired.Interfaces;
using System.Data;

namespace Butterfly.Game.Items.Wired.Actions
{
    public class KickUser : WiredActionBase, IWired, IWiredCycleable, IWiredEffect
    {   
        public KickUser(Item item, Room room) : base(item, room, (int)WiredActionType.KICK_FROM_ROOM)
        {
        }

        public override bool OnCycle(RoomUser user, Item item)
        {
            if (user != null && user.GetClient() != null)
            {
                if (user.RoomId == this.RoomInstance.RoomData.Id)
                {
                    this.RoomInstance.GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true, true);
                }
            }

            return false;
        }

        public override void Handle(RoomUser user, Item item)
        {
            if(this.BeforeCycle(user, item))
                base.Handle(user, item);
        }

        public bool BeforeCycle(RoomUser user, Item item)
        {
            if (user != null && user.GetClient() != null && user.GetClient().GetUser() != null)
            {
                if (user.GetClient().GetUser().HasFuse("fuse_mod") || this.RoomInstance.RoomData.OwnerId == user.UserId)
                {
                    user.SendWhisperChat(ButterflyEnvironment.GetLanguageManager().TryGetValue("wired.kick.exception", user.GetClient().Langue));
                    
                    return false;
                }

                user.ApplyEffect(4);
                user.Freeze = true;

                if (!string.IsNullOrEmpty(this.StringParam))
                {
                    user.SendWhisperChat(this.StringParam);
                }
            }

            return true;
        }

        public void SaveToDatabase(IQueryAdapter dbClient)
        {
            WiredUtillity.SaveTriggerItem(dbClient, this.Id, string.Empty, this.StringParam, false, null, this.Delay);
        }

        public void LoadFromDatabase(DataRow row)
        {
            if (int.TryParse(row["delay"].ToString(), out int delay))
	            this.Delay = delay;
                
            this.StringParam = row["trigger_data"].ToString();
        }
    }
}
