﻿using WibboEmulator.Communication.Packets.Outgoing.Rooms.Avatar;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

namespace WibboEmulator.Games.Chat.Commands.Cmd
{
    internal class RoomDance : IChatCommand
    {
        public void Execute(GameClient Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Please enter a dance ID. (1-4)");
                return;
            }

            int DanceId = Convert.ToInt32(Params[1]);
            if (DanceId < 0 || DanceId > 4)
            {
                Session.SendWhisper("Please enter a dance ID. (1-4)");
                return;
            }

            List<RoomUser> Users = Room.GetRoomUserManager().GetRoomUsers();
            if (Users.Count > 0)
            {
                foreach (RoomUser user in Users.ToList())
                {
                    if (user == null)
                        continue;

                    if (user.CarryItemID > 0)
                        user.CarryItemID = 0;

                    user.DanceId = DanceId;
                    Room.SendPacket(new DanceComposer(user.VirtualId, DanceId));
                }
            }
        }
    }
}