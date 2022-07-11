﻿using WibboEmulator.Game.Clients;
using WibboEmulator.Game.Rooms.PathFinding;
using WibboEmulator.Game.Rooms;

namespace WibboEmulator.Game.Chat.Commands.Cmd
{
    internal class AllEyesOnMe : IChatCommand
    {
        public void Execute(Client Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            RoomUser ThisUser = Room.GetRoomUserManager().GetRoomUserByUserId(Session.GetUser().Id);
            if (ThisUser == null)
                return;

            List<RoomUser> Users = Room.GetRoomUserManager().GetRoomUsers();
            foreach (RoomUser U in Users.ToList())
            {
                if (U == null || Session.GetUser().Id == U.UserId)
                    continue;

                U.SetRot(Rotation.Calculate(U.X, U.Y, ThisUser.X, ThisUser.Y), false);
            }
        }
    }
}
