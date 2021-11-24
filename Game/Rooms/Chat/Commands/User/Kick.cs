﻿using Butterfly.Game.Clients;
using System.Linq;

namespace Butterfly.Game.Rooms.Chat.Commands.Cmd
{
    internal class Kick : IChatCommand
    {
        public void Execute(Client Session, Room Room, RoomUser UserRoom, string[] Params)
        {

            Client clientByUsername = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            if (clientByUsername == null || clientByUsername.GetHabbo() == null)
            {
                return;
            }

            if (Session.GetHabbo().Rank <= clientByUsername.GetHabbo().Rank)
            {
                return;
            }

            if (clientByUsername.GetHabbo().CurrentRoomId < 1U)
            {
                return;
            }

            Room.GetRoomUserManager().RemoveUserFromRoom(clientByUsername, true, true);
        }
    }
}
