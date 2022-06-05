﻿using Wibbo.Communication.Packets.Outgoing.Inventory.Achievements;

using Wibbo.Game.Clients;

namespace Wibbo.Communication.RCON.Commands.User
{
    internal class AddWinwinCommand : IRCONCommand
    {
        public bool TryExecute(string[] parameters)
        {
            if (parameters.Length != 3)
            {
                return false;
            }

            if (!int.TryParse(parameters[1], out int Userid))
            {
                return false;
            }

            if (Userid == 0)
            {
                return false;
            }

            Client Client = WibboEnvironment.GetGame().GetClientManager().GetClientByUserID(Userid);
            if (Client == null)
            {
                return false;
            }

            if (!int.TryParse(parameters[2], out int Winwin))
            {
                return false;
            }

            if (Winwin == 0)
            {
                return false;
            }

            Client.GetUser().AchievementPoints = Client.GetUser().AchievementPoints + Winwin;
            Client.SendPacket(new AchievementScoreComposer(Client.GetUser().AchievementPoints));

            return true;
        }
    }
}
