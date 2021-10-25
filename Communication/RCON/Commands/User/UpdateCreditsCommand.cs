﻿using Butterfly.Database.Interfaces;
using Butterfly.HabboHotel.GameClients;
using System;
using System.Data;

namespace Butterfly.Communication.RCON.Commands.User
{
    internal class UpdateCreditsCommand : IRCONCommand
    {
        public bool TryExecute(string[] parameters)
        {
            if (parameters.Length != 2)
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

            GameClient Client = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUserID(Userid);
            if (Client == null)
            {
                return false;
            }

            DataRow row;
            using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT credits FROM users WHERE id = @userid");
                dbClient.AddParameter("userid", Client.GetHabbo().Id);
                row = dbClient.GetRow();
            }
            Client.GetHabbo().Credits = Convert.ToInt32(row["credits"]);
            Client.GetHabbo().UpdateCreditsBalance();

            return true;
        }
    }
}
