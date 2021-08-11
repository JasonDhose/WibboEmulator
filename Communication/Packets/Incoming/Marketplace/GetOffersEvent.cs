﻿using Butterfly.Communication.Packets.Outgoing;
using Butterfly.Communication.Packets.Outgoing.Avatar;
using Butterfly.Communication.Packets.Outgoing.BuildersClub;
using Butterfly.Communication.Packets.Outgoing.Camera;
using Butterfly.Communication.Packets.Outgoing.Catalog;
using Butterfly.Communication.Packets.Outgoing.GameCenter;
using Butterfly.Communication.Packets.Outgoing.Groups;
using Butterfly.Communication.Packets.Outgoing.Handshake;
using Butterfly.Communication.Packets.Outgoing.Help;
using Butterfly.Communication.Packets.Outgoing.Inventory;
using Butterfly.Communication.Packets.Outgoing.Inventory.Achievements;
using Butterfly.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Butterfly.Communication.Packets.Outgoing.Inventory.Badges;
using Butterfly.Communication.Packets.Outgoing.Inventory.Bots;
using Butterfly.Communication.Packets.Outgoing.Inventory.Furni;
using Butterfly.Communication.Packets.Outgoing.Inventory.Pets;
using Butterfly.Communication.Packets.Outgoing.Inventory.Purse;
using Butterfly.Communication.Packets.Outgoing.Inventory.Trading;
using Butterfly.Communication.Packets.Outgoing.LandingView;
using Butterfly.Communication.Packets.Outgoing.MarketPlace;
using Butterfly.Communication.Packets.Outgoing.Messenger;
using Butterfly.Communication.Packets.Outgoing.Misc;
using Butterfly.Communication.Packets.Outgoing.Moderation;
using Butterfly.Communication.Packets.Outgoing.Navigator;
using Butterfly.Communication.Packets.Outgoing.Navigator.New;
using Butterfly.Communication.Packets.Outgoing.Notifications;
using Butterfly.Communication.Packets.Outgoing.Pets;
using Butterfly.Communication.Packets.Outgoing.Quests;
using Butterfly.Communication.Packets.Outgoing.Rooms;
using Butterfly.Communication.Packets.Outgoing.Rooms.Action;
using Butterfly.Communication.Packets.Outgoing.Rooms.AI.Bots;
using Butterfly.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Butterfly.Communication.Packets.Outgoing.Rooms.Avatar;
using Butterfly.Communication.Packets.Outgoing.Rooms.Chat;
using Butterfly.Communication.Packets.Outgoing.Rooms.Engine;
using Butterfly.Communication.Packets.Outgoing.Rooms.FloorPlan;
using Butterfly.Communication.Packets.Outgoing.Rooms.Freeze;
using Butterfly.Communication.Packets.Outgoing.Rooms.Furni;
using Butterfly.Communication.Packets.Outgoing.Rooms.Furni.Furni;
using Butterfly.Communication.Packets.Outgoing.Rooms.Furni.Moodlight;
using Butterfly.Communication.Packets.Outgoing.Rooms.Furni.Stickys;
using Butterfly.Communication.Packets.Outgoing.Rooms.Furni.Wired;
using Butterfly.Communication.Packets.Outgoing.Rooms.Notifications;
using Butterfly.Communication.Packets.Outgoing.Rooms.Permissions;
using Butterfly.Communication.Packets.Outgoing.Rooms.Session;
using Butterfly.Communication.Packets.Outgoing.Rooms.Settings;
using Butterfly.Communication.Packets.Outgoing.Rooms.Wireds;
using Butterfly.Communication.Packets.Outgoing.Sound;
using Butterfly.Communication.Packets.Outgoing.Users;
using Butterfly.Communication.Packets.Outgoing.WebSocket;
using Butterfly.Database.Interfaces;
using Butterfly.HabboHotel.Catalog.Marketplace;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Butterfly.Communication.Packets.Incoming.Marketplace
{
    internal class GetOffersEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int MinCost = Packet.PopInt();
            int MaxCost = Packet.PopInt();
            string SearchQuery = Packet.PopString();
            int FilterMode = Packet.PopInt();

            DataTable table = null;
            StringBuilder builder = new StringBuilder();
            string str = "";
            builder.Append("WHERE `state` = '1' AND `timestamp` >= " + ButterflyEnvironment.GetGame().GetCatalog().GetMarketplace().FormatTimestamp().ToString());
            if (MinCost >= 0)
            {
                builder.Append(" AND `total_price` > " + MinCost);
            }

            if (MaxCost >= 0)
            {
                builder.Append(" AND `total_price` < " + MaxCost);
            }

            switch (FilterMode)
            {
                case 1:
                    str = "ORDER BY `asking_price` DESC";
                    break;

                default:
                    str = "ORDER BY `asking_price` ASC";
                    break;
            }

            if (SearchQuery.Length >= 1)
            {
                builder.Append(" AND `public_name` LIKE @search_query");
            }

            using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `offer_id`, `item_type`, `sprite_id`, `total_price`, `limited_number`,`limited_stack` FROM `catalog_marketplace_offers` " + builder.ToString() + " " + str + " LIMIT 500");
                dbClient.AddParameter("search_query", SearchQuery.Replace("%", "\\%").Replace("_", "\\_") + "%");

                table = dbClient.GetTable();
            }

            ButterflyEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItems.Clear();
            ButterflyEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Clear();
            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    if (!ButterflyEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Contains(Convert.ToInt32(row["offer_id"])))
                    {
                        ButterflyEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Add(Convert.ToInt32(row["offer_id"]));
                        ButterflyEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItems.Add(new MarketOffer(Convert.ToInt32(row["offer_id"]), Convert.ToInt32(row["sprite_id"]), Convert.ToInt32(row["total_price"]), int.Parse(row["item_type"].ToString()), Convert.ToInt32(row["limited_number"]), Convert.ToInt32(row["limited_stack"])));
                    }
                }
            }

            Dictionary<int, MarketOffer> dictionary = new Dictionary<int, MarketOffer>();
            Dictionary<int, int> dictionary2 = new Dictionary<int, int>();

            foreach (MarketOffer item in ButterflyEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItems)
            {
                if (dictionary.ContainsKey(item.SpriteId))
                {
                    if (item.LimitedNumber > 0)
                    {
                        if (!dictionary.ContainsKey(item.OfferID))
                        {
                            dictionary.Add(item.OfferID, item);
                        }

                        if (!dictionary2.ContainsKey(item.OfferID))
                        {
                            dictionary2.Add(item.OfferID, 1);
                        }
                    }
                    else
                    {
                        if (dictionary[item.SpriteId].TotalPrice > item.TotalPrice)
                        {
                            dictionary.Remove(item.SpriteId);
                            dictionary.Add(item.SpriteId, item);
                        }

                        int num = dictionary2[item.SpriteId];
                        dictionary2.Remove(item.SpriteId);
                        dictionary2.Add(item.SpriteId, num + 1);
                    }
                }
                else
                {
                    if (!dictionary.ContainsKey(item.SpriteId))
                    {
                        dictionary.Add(item.SpriteId, item);
                    }

                    if (!dictionary2.ContainsKey(item.SpriteId))
                    {
                        dictionary2.Add(item.SpriteId, 1);
                    }
                }
            }

            Session.SendPacket(new MarketPlaceOffersComposer(MinCost, MaxCost, dictionary, dictionary2));
        }
    }
}
