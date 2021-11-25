﻿using Butterfly.Communication.Packets.Outgoing;
using Butterfly.Communication.Packets.Outgoing.Inventory.Furni;
using Butterfly.Core;
using Butterfly.Database.Daos;
using Butterfly.Database.Interfaces;
using Butterfly.Game.Items;
using System;
using System.Collections.Generic;

namespace Butterfly.Game.Rooms
{
    public class Trade
    {
        private readonly TradeUser[] _users;
        private int _tradeStage;
        private readonly int _roomId;
        private readonly int _oneId;
        private readonly int _twoId;

        public bool AllUsersAccepted
        {
            get
            {
                for (int index = 0; index < this._users.Length; ++index)
                {
                    if (this._users[index] != null && !this._users[index].HasAccepted)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public Trade(int UserOneId, int UserTwoId, int RoomId)
        {
            this._oneId = UserOneId;
            this._twoId = UserTwoId;
            this._users = new TradeUser[2];
            this._users[0] = new TradeUser(UserOneId, RoomId);
            this._users[1] = new TradeUser(UserTwoId, RoomId);
            this._tradeStage = 1;
            this._roomId = RoomId;

            ServerPacket Message = new ServerPacket(ServerPacketHeader.TRADE);
            Message.WriteInteger(UserOneId);
            Message.WriteInteger(1);
            Message.WriteInteger(UserTwoId);
            Message.WriteInteger(1);
            this.SendMessageToUsers(Message);

            foreach (TradeUser tradeUser in this._users)
            {
                if (!tradeUser.GetRoomUser().Statusses.ContainsKey("/trd"))
                {
                    tradeUser.GetRoomUser().SetStatus("/trd", "");
                    tradeUser.GetRoomUser().UpdateNeeded = true;
                }
            }
        }

        public bool ContainsUser(int Id)
        {
            for (int index = 0; index < this._users.Length; ++index)
            {
                if (this._users[index] != null && this._users[index].UserId == Id)
                {
                    return true;
                }
            }
            return false;
        }

        public TradeUser GetTradeUser(int Id)
        {
            for (int index = 0; index < this._users.Length; ++index)
            {
                if (this._users[index] != null && this._users[index].UserId == Id)
                {
                    return this._users[index];
                }
            }
            return null;
        }

        public void OfferItem(int UserId, Item Item, bool UpdateWindows = true)
        {
            TradeUser tradeUser = this.GetTradeUser(UserId);
            if (tradeUser == null || Item == null || (!Item.GetBaseItem().AllowTrade || tradeUser.HasAccepted) || this._tradeStage != 1)
            {
                return;
            }

            this.ClearAccepted();
            if (!tradeUser.OfferedItems.Contains(Item))
            {
                tradeUser.OfferedItems.Add(Item);
            }

            if (UpdateWindows)
            {
                this.UpdateTradeWindow();
            }
        }

        public void TakeBackItem(int UserId, Item Item)
        {
            TradeUser tradeUser = this.GetTradeUser(UserId);
            if (tradeUser == null || Item == null || (tradeUser.HasAccepted || this._tradeStage != 1))
            {
                return;
            }

            this.ClearAccepted();
            tradeUser.OfferedItems.Remove(Item);
            this.UpdateTradeWindow();
        }

        public void Accept(int UserId)
        {
            TradeUser tradeUser = this.GetTradeUser(UserId);
            if (tradeUser == null || this._tradeStage != 1)
            {
                return;
            }

            tradeUser.HasAccepted = true;
            ServerPacket Message = new ServerPacket(ServerPacketHeader.TRADE_ACCEPTED);
            Message.WriteInteger(UserId);
            Message.WriteInteger(1);
            this.SendMessageToUsers(Message);
            if (!this.AllUsersAccepted)
            {
                return;
            }

            this.SendMessageToUsers(new ServerPacket(ServerPacketHeader.TRADE_CONFIRM));
            this._tradeStage++;
            this.ClearAccepted();
        }

        public void Unaccept(int UserId)
        {
            TradeUser tradeUser = this.GetTradeUser(UserId);
            if (tradeUser == null || this._tradeStage != 1 || this.AllUsersAccepted)
            {
                return;
            }

            tradeUser.HasAccepted = false;
            ServerPacket Message = new ServerPacket(ServerPacketHeader.TRADE_ACCEPTED);
            Message.WriteInteger(UserId);
            Message.WriteInteger(0);
            this.SendMessageToUsers(Message);
        }

        public void CompleteTrade(int UserId)
        {
            TradeUser tradeUser = this.GetTradeUser(UserId);
            if (tradeUser == null || this._tradeStage != 2)
            {
                return;
            }

            tradeUser.HasAccepted = true;

            ServerPacket Message = new ServerPacket(ServerPacketHeader.TRADE_ACCEPTED);
            Message.WriteInteger(UserId);
            Message.WriteInteger(1);
            this.SendMessageToUsers(Message);

            if (!this.AllUsersAccepted)
            {
                return;
            }

            this._tradeStage = 999;
            this.EndTrade();
        }

        private void EndTrade()
        {
            try
            {
                if (this.DeliverItems())
                {
                    this.SaveLogs();
                }

                this.CloseTradeClean();
            }
            catch (Exception ex)
            {
                Logging.LogThreadException((ex).ToString(), "Trade task");
            }
        }

        public void ClearAccepted()
        {
            foreach (TradeUser tradeUser in this._users)
            {
                tradeUser.HasAccepted = false;
            }
        }

        public void UpdateTradeWindow()
        {
            ServerPacket Message = new ServerPacket(ServerPacketHeader.TRADE_UPDATE);
            for (int index = 0; index < this._users.Length; ++index)
            {
                TradeUser tradeUser = this._users[index];
                if (tradeUser != null)
                {
                    Message.WriteInteger(tradeUser.UserId);
                    Message.WriteInteger(tradeUser.OfferedItems.Count);
                    foreach (Item userItem in tradeUser.OfferedItems)
                    {
                        Message.WriteInteger(userItem.Id);
                        Message.WriteString(userItem.GetBaseItem().Type.ToString().ToLower());
                        Message.WriteInteger(userItem.Id);
                        Message.WriteInteger(userItem.GetBaseItem().SpriteId);
                        Message.WriteInteger(0);
                        if (userItem.Limited > 0)
                        {
                            Message.WriteBoolean(false);
                            Message.WriteInteger(256);
                            Message.WriteString("");
                            Message.WriteInteger(userItem.Limited);
                            Message.WriteInteger(userItem.LimitedStack);
                        }
                        else if (userItem.GetBaseItem().InteractionType == InteractionType.BADGE_DISPLAY || userItem.GetBaseItem().InteractionType == InteractionType.BADGE_TROC)
                        {
                            Message.WriteBoolean(false);
                            Message.WriteInteger(2);
                            Message.WriteInteger(4);

                            if (userItem.ExtraData.Contains(Convert.ToChar(9).ToString()))
                            {
                                string[] BadgeData = userItem.ExtraData.Split(Convert.ToChar(9));

                                Message.WriteString("0");//No idea
                                Message.WriteString(BadgeData[0]);//Badge name
                                Message.WriteString(BadgeData[1]);//Owner
                                Message.WriteString(BadgeData[2]);//Date
                            }
                            else
                            {
                                Message.WriteString("0");//No idea
                                Message.WriteString(userItem.ExtraData);//Badge name
                                Message.WriteString("");//Owner
                                Message.WriteString("");//Date
                            }
                        }
                        else
                        {
                            Message.WriteBoolean(true);
                            Message.WriteInteger(0);
                            Message.WriteString("");
                        }
                        Message.WriteInteger(0);
                        Message.WriteInteger(0);
                        Message.WriteInteger(0);
                        if (userItem.GetBaseItem().Type == 's')
                        {
                            Message.WriteInteger(0);
                        }
                    }
                    Message.WriteInteger(tradeUser.OfferedItems.Count);
                    Message.WriteInteger(0);

                }
            }
            this.SendMessageToUsers(Message);
        }

        public bool DeliverItems()
        {
            List<Item> list1 = this.GetTradeUser(this._oneId).OfferedItems;
            List<Item> list2 = this.GetTradeUser(this._twoId).OfferedItems;

            foreach (Item userItem in list1)
            {
                if (this.GetTradeUser(this._oneId).GetClient().GetHabbo().GetInventoryComponent().GetItem(userItem.Id) == null)
                {
                    this.GetTradeUser(this._oneId).GetClient().SendNotification(ButterflyEnvironment.GetLanguageManager().TryGetValue("trade.failed", this.GetTradeUser(this._oneId).GetClient().Langue));
                    this.GetTradeUser(this._twoId).GetClient().SendNotification(ButterflyEnvironment.GetLanguageManager().TryGetValue("trade.failed", this.GetTradeUser(this._twoId).GetClient().Langue));
                    return false;
                }
            }

            foreach (Item userItem in list2)
            {
                if (this.GetTradeUser(this._twoId).GetClient().GetHabbo().GetInventoryComponent().GetItem(userItem.Id) == null)
                {
                    this.GetTradeUser(this._oneId).GetClient().SendNotification(ButterflyEnvironment.GetLanguageManager().TryGetValue("trade.failed", this.GetTradeUser(this._oneId).GetClient().Langue));
                    this.GetTradeUser(this._twoId).GetClient().SendNotification(ButterflyEnvironment.GetLanguageManager().TryGetValue("trade.failed", this.GetTradeUser(this._twoId).GetClient().Langue));
                    return false;
                }
            }

            foreach (Item userItem in list1)
            {
                this.GetTradeUser(this._oneId).GetClient().GetHabbo().GetInventoryComponent().RemoveItem(userItem.Id);
                this.GetTradeUser(this._twoId).GetClient().GetHabbo().GetInventoryComponent().AddItem(userItem);
            }

            foreach (Item userItem in list2)
            {
                this.GetTradeUser(this._twoId).GetClient().GetHabbo().GetInventoryComponent().RemoveItem(userItem.Id);
                this.GetTradeUser(this._oneId).GetClient().GetHabbo().GetInventoryComponent().AddItem(userItem);
            }

            ServerPacket Message1 = new ServerPacket(ServerPacketHeader.UNSEEN_ITEMS);
            Message1.WriteInteger(1);
            int i1 = 1;
            foreach (Item userItem in list1)
            {
                if (userItem.GetBaseItem().Type.ToString().ToLower() != "s")
                {
                    i1 = 2;
                }
            }
            Message1.WriteInteger(i1);
            Message1.WriteInteger(list1.Count);
            foreach (Item userItem in list1)
            {
                Message1.WriteInteger(userItem.Id);
            }

            this.GetTradeUser(this._twoId).GetClient().SendPacket(Message1);

            ServerPacket Message2 = new ServerPacket(ServerPacketHeader.UNSEEN_ITEMS);
            Message2.WriteInteger(1);
            int i2 = 1;
            foreach (Item userItem in list2)
            {
                if (userItem.GetBaseItem().Type.ToString().ToLower() != "s")
                {
                    i2 = 2;
                }
            }
            Message2.WriteInteger(i2);
            Message2.WriteInteger(list2.Count);
            foreach (Item userItem in list2)
            {
                Message2.WriteInteger(userItem.Id);
            }

            this.GetTradeUser(this._oneId).GetClient().SendPacket(Message2);

            this.GetTradeUser(this._oneId).GetClient().SendPacket(new FurniListUpdateComposer());
            this.GetTradeUser(this._twoId).GetClient().SendPacket(new FurniListUpdateComposer());

            return true;
        }

        private void SaveLogs()
        {
            Dictionary<string, int> ItemsOneCounter = new Dictionary<string, int>();
            Dictionary<string, int> ItemsTwoCounter = new Dictionary<string, int>();

            foreach (Item userItem in this.GetTradeUser(this._oneId).OfferedItems)
            {
                if (!ItemsOneCounter.ContainsKey(userItem.GetBaseItem().ItemName))
                {
                    ItemsOneCounter.Add(userItem.GetBaseItem().ItemName, 1);
                }
                else
                {
                    ItemsOneCounter[userItem.GetBaseItem().ItemName]++;
                }
            }

            foreach (Item userItem in this.GetTradeUser(this._twoId).OfferedItems)
            {
                if (!ItemsTwoCounter.ContainsKey(userItem.GetBaseItem().ItemName))
                {
                    ItemsTwoCounter.Add(userItem.GetBaseItem().ItemName, 1);
                }
                else
                {
                    ItemsTwoCounter[userItem.GetBaseItem().ItemName]++;
                }
            }

            string LogsOneString = "";
            foreach (KeyValuePair<string, int> Logs in ItemsOneCounter)
            {
                LogsOneString += $"{Logs.Key} ({Logs.Value}),";
            }

            LogsOneString = LogsOneString.TrimEnd(',');

            string LogsTwoString = "";
            foreach (KeyValuePair<string, int> Logs in ItemsTwoCounter)
            {
                LogsTwoString += $"{Logs.Key} ({Logs.Value}),";
            }

            LogsTwoString = LogsTwoString.TrimEnd(',');

            using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                LogTradeDao.Insert(dbClient, this._oneId, this._twoId, LogsOneString, LogsTwoString, this._roomId);
            }
        }

        public void CloseTradeClean()
        {
            for (int index = 0; index < this._users.Length; ++index)
            {
                TradeUser tradeUser = this._users[index];
                if (tradeUser != null && tradeUser.GetRoomUser() != null)
                {
                    tradeUser.GetRoomUser().RemoveStatus("/trd");
                    tradeUser.GetRoomUser().UpdateNeeded = true;
                }
            }
            this.SendMessageToUsers(new ServerPacket(ServerPacketHeader.TRADE_CLOSE));
            this.GetRoom().ActiveTrades.Remove(this);
        }

        public void CloseTrade(int UserId)
        {
            for (int index = 0; index < this._users.Length; ++index)
            {
                TradeUser tradeUser = this._users[index];
                if (tradeUser != null && tradeUser.GetRoomUser() != null)
                {
                    tradeUser.GetRoomUser().RemoveStatus("/trd");
                    tradeUser.GetRoomUser().UpdateNeeded = true;
                }
            }
            ServerPacket Message = new ServerPacket(ServerPacketHeader.TRADE_CLOSED);
            Message.WriteInteger(UserId);
            Message.WriteInteger(2);
            this.SendMessageToUsers(Message);
        }

        public void SendMessageToUsers(ServerPacket Message)
        {
            if (this._users == null)
            {
                return;
            }

            for (int index = 0; index < this._users.Length; ++index)
            {
                TradeUser tradeUser = this._users[index];
                if (tradeUser != null && tradeUser != null && tradeUser.GetClient() != null)
                {
                    tradeUser.GetClient().SendPacket(Message);
                }
            }
        }

        private Room GetRoom()
        {
            return ButterflyEnvironment.GetGame().GetRoomManager().GetRoom(this._roomId);
        }
    }
}