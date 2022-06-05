﻿using Butterfly.Communication.Packets.Outgoing.Inventory.Purse;
using Butterfly.Database.Daos;
using Butterfly.Database.Interfaces;
using Butterfly.Game.Clients;
using System.Data;

namespace Butterfly.Communication.Packets.Incoming.Marketplace
{
    internal class RedeemOfferCreditsEvent : IPacketEvent
    {
        public double Delay => 1000;

        public void Parse(Client Session, ClientPacket Packet)
        {
            int CreditsOwed = 0;

            DataTable Table = null;
            using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
                Table = CatalogMarketplaceOfferDao.GetPriceByUserId(dbClient, Session.GetUser().Id);

            if (Table != null)
            {
                foreach (DataRow row in Table.Rows)
                {
                    CreditsOwed += Convert.ToInt32(row["asking_price"]);
                }

                if (CreditsOwed >= 1)
                {
                    Session.GetUser().WibboPoints += CreditsOwed;
                    Session.SendPacket(new ActivityPointNotificationComposer(Session.GetUser().WibboPoints, 0, 105));

                    using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        CatalogMarketplaceOfferDao.Delete(dbClient, Session.GetUser().Id);
                        UserDao.UpdateAddPoints(dbClient, Session.GetUser().Id, CreditsOwed);
                    }
                }
            }
        }
    }
}