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

using Butterfly.Core;
using Butterfly.Database.Interfaces;
using Butterfly.HabboHotel.ChatMessageStorage;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Rooms;
using Butterfly.HabboHotel.Users;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Butterfly.HabboHotel.Support
{
    public class ModerationManager
    {
        private readonly List<SupportTicket> _tickets;
        private readonly List<string> _userMessagePresets;
        private readonly List<string> _roomMessagePresets;

        private readonly List<TicketResolution> _ticketResolution1;
        private readonly List<TicketResolution> _ticketResolution2;

        private readonly Dictionary<int, string> _moderationCFHTopics;

        private readonly Dictionary<int, List<ModerationPresetActions>> _moderationCFHTopicActions;

        public ModerationManager()
        {
            this._tickets = new List<SupportTicket>();
            this._userMessagePresets = new List<string>();
            this._roomMessagePresets = new List<string>();
            this._ticketResolution1 = new List<TicketResolution>();
            this._ticketResolution2 = new List<TicketResolution>();
            this._moderationCFHTopics = new Dictionary<int, string>();
            this._moderationCFHTopicActions = new Dictionary<int, List<ModerationPresetActions>>();
        }

        public void Init()
        {
            this.LoadMessageTopics();
            this.LoadMessagePresets();
            this.LoadPendingTickets();
            this.LoadTicketResolution();
        }

        public ServerPacket SerializeTool()
        {
            ServerPacket Response = new ServerPacket(ServerPacketHeader.MODERATION_TOOL);
            Response.WriteInteger(this._tickets.Count);
            foreach (SupportTicket ticket in this._tickets)
            {
                ticket.Serialize(Response);
            }

            Response.WriteInteger(this._userMessagePresets.Count);
            foreach (string Preset in this._userMessagePresets)
            {
                Response.WriteString(Preset);
            }

            Response.WriteInteger(0); // Thing presets

            /*
            Response.WriteString("Sexually Explicit"); //catégorie titre
            Response.WriteBoolean(true);

            Response.WriteInteger(TicketResolution1.Count); //nombre
            foreach (TicketResolution Resolu in TicketResolution1)
            {
                Response.WriteString(Resolu.Titre); //Titre onglet
                Response.WriteString(Resolu.Soustitre); //sous titre
                Response.WriteInteger(Resolu.Ban_hours); //ban temps si + de 10 000 = permanant
                Response.WriteInteger(Resolu.Enablemute); //0 = disabed / 1 = enable
                Response.WriteInteger(Resolu.Mute_hours); //mute hours
                Response.WriteInteger(Resolu.Reminder); // reminder_text ?
                Response.WriteString(Resolu.Message);
                Response.WriteBoolean(true); //
            }
            Response.WriteString("PII"); //catégorie titre
            Response.WriteBoolean(true);

            Response.WriteInteger(TicketResolution2.Count); //nombre
            foreach (TicketResolution Resolu in TicketResolution2)
            {
                Response.WriteString(Resolu.Titre); //Titre onglet
                Response.WriteString(Resolu.Soustitre); //sous titre
                Response.WriteInteger(Resolu.Ban_hours); //ban temps si + de 10 000 = permanant
                Response.WriteInteger(Resolu.Enablemute); //0 = disabed / 1 = enable
                Response.WriteInteger(Resolu.Mute_hours); //mute hours
                Response.WriteInteger(Resolu.Reminder); // reminder_text ?
                Response.WriteString(Resolu.Message);
                Response.WriteBoolean(true); //
            }*/

            // Permissions
            Response.WriteBoolean(true); // ticket_queue fuse
            Response.WriteBoolean(true); // chatlog fuse
            Response.WriteBoolean(true); // message / caution fuse
            Response.WriteBoolean(true); // kick fuse
            Response.WriteBoolean(true); // band fuse
            Response.WriteBoolean(true); // broadcastshit fuse
            Response.WriteBoolean(true);

            // Room tool notices
            Response.WriteInteger(this._roomMessagePresets.Count);
            foreach (string Preset in this._roomMessagePresets)
            {
                Response.WriteString(Preset);
            }
            return Response;
        }

        public Dictionary<string, List<ModerationPresetActions>> UserActionPresets
        {
            get
            {
                Dictionary<string, List<ModerationPresetActions>> Result = new Dictionary<string, List<ModerationPresetActions>>();
                foreach (KeyValuePair<int, string> Category in this._moderationCFHTopics.ToList())
                {
                    Result.Add(Category.Value, new List<ModerationPresetActions>());

                    if (this._moderationCFHTopicActions.ContainsKey(Category.Key))
                    {
                        foreach (ModerationPresetActions Data in this._moderationCFHTopicActions[Category.Key])
                        {
                            Result[Category.Value].Add(Data);
                        }
                    }
                }
                return Result;
            }
        }

        public void LoadMessageTopics()
        {
            using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable ModerationTopics = null;
                dbClient.SetQuery("SELECT * FROM moderation_topics");
                ModerationTopics = dbClient.GetTable();

                if (ModerationTopics != null)
                {
                    foreach (DataRow Row in ModerationTopics.Rows)
                    {
                        if (!this._moderationCFHTopics.ContainsKey(Convert.ToInt32(Row["id"])))
                        {
                            this._moderationCFHTopics.Add(Convert.ToInt32(Row["id"]), Convert.ToString(Row["caption"]));
                        }
                    }
                }

                DataTable ModerationTopicsActions = null;
                dbClient.SetQuery("SELECT * FROM moderation_topic_actions");
                ModerationTopicsActions = dbClient.GetTable();

                if (ModerationTopicsActions != null)
                {
                    foreach (DataRow Row in ModerationTopicsActions.Rows)
                    {
                        int ParentId = Convert.ToInt32(Row["parent_id"]);

                        if (!this._moderationCFHTopicActions.ContainsKey(ParentId))
                        {
                            this._moderationCFHTopicActions.Add(ParentId, new List<ModerationPresetActions>());
                        }

                        this._moderationCFHTopicActions[ParentId].Add(new ModerationPresetActions(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["parent_id"]), Convert.ToString(Row["type"]), Convert.ToString(Row["caption"]), Convert.ToString(Row["message_text"]),
                            Convert.ToInt32(Row["mute_time"]), Convert.ToInt32(Row["ban_time"]), Convert.ToInt32(Row["ip_time"]), Convert.ToInt32(Row["trade_lock_time"]), Convert.ToString(Row["default_sanction"])));
                    }
                }
            }
        }

        public void LoadMessagePresets()
        {
            this._userMessagePresets.Clear();
            this._roomMessagePresets.Clear();
            using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT type,message FROM moderation_presets WHERE enabled = '1'");
                DataTable table = dbClient.GetTable();
                foreach (DataRow dataRow in table.Rows)
                {
                    string str = (string)dataRow["message"];
                    switch (dataRow["type"].ToString().ToLower())
                    {
                        case "message":
                            this._userMessagePresets.Add(str);
                            continue;
                        case "roommessage":
                            this._roomMessagePresets.Add(str);
                            continue;
                        default:
                            continue;
                    }
                }
            }
        }

        public void LoadTicketResolution()
        {
            this._ticketResolution1.Clear();
            this._ticketResolution2.Clear();
            using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM moderation_resolution");
                DataTable table = dbClient.GetTable();
                foreach (DataRow dataRow in table.Rows)
                {
                    TicketResolution str = new TicketResolution((string)dataRow["title"], (string)dataRow["subtitle"], Convert.ToInt32(dataRow["ban_hours"]), Convert.ToInt32(dataRow["enable_mute"]), Convert.ToInt32(dataRow["mute_hours"]), Convert.ToInt32(dataRow["reminder"]), (string)dataRow["message"]);
                    switch (dataRow["type"].ToString())
                    {
                        case "Sexual":
                            this._ticketResolution1.Add(str);
                            continue;
                        case "PII":
                            this._ticketResolution2.Add(str);
                            continue;
                        default:
                            continue;
                    }
                }
            }
        }

        public void LoadPendingTickets()
        {
            using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM moderation_tickets WHERE status = 'open'");
                DataTable table = dbClient.GetTable();
                if (table == null)
                {
                    return;
                }

                foreach (DataRow dataRow in table.Rows)
                {
                    SupportTicket supportTicket = new SupportTicket(Convert.ToInt32(dataRow["id"]), Convert.ToInt32(dataRow["score"]), Convert.ToInt32(dataRow["type"]), Convert.ToInt32(dataRow["sender_id"]), Convert.ToInt32(dataRow["reported_id"]), (string)dataRow["message"], Convert.ToInt32(dataRow["room_id"]), (string)dataRow["room_name"], (double)dataRow["timestamp"]);
                    if (dataRow["status"].ToString().ToLower() == "picked")
                    {
                        supportTicket.Pick(Convert.ToInt32(dataRow["moderator_id"]), false);
                    }

                    this._tickets.Add(supportTicket);
                }
            }
        }

        public void SendNewTicket(GameClient Session, int Category, int ReportedUser, string Message)
        {
            RoomData roomData = ButterflyEnvironment.GetGame().GetRoomManager().GenerateNullableRoomData(Session.GetHabbo().CurrentRoomId);
            int Id = 0;
            string roomname = (roomData == null) ? roomData.Name : "Aucun appart";
            int roomid = (roomData == null) ? roomData.Id : 0;
            using (IQueryAdapter queryreactor = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                queryreactor.SetQuery("INSERT INTO moderation_tickets (score,type,status,sender_id,reported_id,moderator_id,message,room_id,room_name,timestamp) VALUES (1,'" + Category + "','open','" + Session.GetHabbo().Id + "','" + ReportedUser + "','0',@message,'" + roomData.Id + "',@name,'" + ButterflyEnvironment.GetUnixTimestamp() + "')");
                queryreactor.AddParameter("message", Message);
                queryreactor.AddParameter("name", roomname);
                Id = Convert.ToInt32(queryreactor.InsertQuery());
            }
            SupportTicket Ticket = new SupportTicket(Id, 1, Category, Session.GetHabbo().Id, ReportedUser, Message, roomid, roomname, ButterflyEnvironment.GetUnixTimestamp());
            this._tickets.Add(Ticket);
            SendTicketToModerators(Ticket);
        }

        public void ApplySanction(GameClient Session, int ReportedUser)
        {
            if (ReportedUser == 0)
            {
                return;
            }

            Habbo UserReport = ButterflyEnvironment.GetHabboById(ReportedUser);
            if (UserReport == null)
            {
                return;
            }

            Session.GetHabbo().GetMessenger().DestroyFriendship(UserReport.Id);

            ServerPacket Response = new ServerPacket(ServerPacketHeader.USER_IGNORED_UPDATE);
            Response.WriteInteger(1);
            Response.WriteString(UserReport.Username);
            Session.SendPacket(Response);

            Room room = ButterflyEnvironment.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
            if (room == null || (room.RoomData.BanFuse != 1 || !room.CheckRights(Session)) && !room.CheckRights(Session, true))
            {
                return;
            }

            RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabboId(UserReport.Id);
            if (roomUserByHabbo == null || roomUserByHabbo.IsBot || (room.CheckRights(roomUserByHabbo.GetClient(), true) || roomUserByHabbo.GetClient().GetHabbo().HasFuse("fuse_mod") || roomUserByHabbo.GetClient().GetHabbo().HasFuse("fuse_no_kick")))
            {
                return;
            }

            room.AddBan(UserReport.Id, 429496729);
            room.GetRoomUserManager().RemoveUserFromRoom(roomUserByHabbo.GetClient(), true, true);
        }

        public SupportTicket GetTicket(int TicketId)
        {
            foreach (SupportTicket supportTicket in this._tickets)
            {
                if (supportTicket.TicketId == TicketId)
                {
                    return supportTicket;
                }
            }
            return null;
        }

        public void PickTicket(GameClient Session, int TicketId)
        {
            SupportTicket ticket = this.GetTicket(TicketId);
            if (ticket == null || ticket.Status != TicketStatus.OPEN)
            {
                return;
            }

            ticket.Pick(Session.GetHabbo().Id, true);
            SendTicketToModerators(ticket);
        }

        public void ReleaseTicket(GameClient Session, int TicketId)
        {
            SupportTicket ticket = this.GetTicket(TicketId);
            if (ticket == null || ticket.Status != TicketStatus.PICKED || ticket.ModeratorId != Session.GetHabbo().Id)
            {
                return;
            }

            ticket.Release(true);
            SendTicketToModerators(ticket);
        }

        public void CloseTicket(GameClient Session, int TicketId, int Result)
        {
            SupportTicket ticket = this.GetTicket(TicketId);
            if (ticket == null || ticket.Status != TicketStatus.PICKED || ticket.ModeratorId != Session.GetHabbo().Id)
            {
                return;
            }

            GameClient clientByUserId = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUserID(ticket.SenderId);

            TicketStatus NewStatus;
            string MessageAlert = "";
            switch (Result)
            {
                case 1:
                    NewStatus = TicketStatus.INVALID;
                    MessageAlert = "Es-tu certain d'avoir bien utilisé cet outil ? Nous voulons donner le meilleur des services mais nous devons aussi aider d'autres personnes dans l'urgence...";
                    break;
                case 2:
                    NewStatus = TicketStatus.ABUSIVE;
                    MessageAlert = "Merci de ne pas utiliser l'outil d'appel à l'aide pour rien. Tu risques l'exclusion.";
                    break;
                default:
                    NewStatus = TicketStatus.RESOLVED;
                    MessageAlert = "Merci, ton souci est résolu ou en cours de résolution. N'hésite pas à Ignorer la personne  ou à la supprimer de ta console s'il s'agit d'insultes.";
                    break;
            }
            if (clientByUserId != null)
            {
                ServerPacket Message = new ServerPacket(ServerPacketHeader.ModToolIssueResponseAlertComposer);
                Message.WriteString(MessageAlert);
                clientByUserId.SendPacket(Message);
            }
            ticket.Close(NewStatus, true);
            SendTicketToModerators(ticket);
        }

        public bool UsersHasPendingTicket(int Id)
        {
            foreach (SupportTicket supportTicket in this._tickets)
            {
                if (supportTicket.SenderId == Id && supportTicket.Status == TicketStatus.OPEN)
                {
                    return true;
                }
            }
            return false;
        }

        public void DeletePendingTicketForUser(int Id)
        {
            foreach (SupportTicket Ticket in this._tickets)
            {
                if (Ticket.SenderId == Id)
                {
                    Ticket.Delete(true);
                    SendTicketToModerators(Ticket);
                    break;
                }
            }
        }

        public static void SendTicketToModerators(SupportTicket Ticket)
        {
            ButterflyEnvironment.GetGame().GetClientManager().SendMessageStaff(Ticket.Serialize());
        }

        public void LogStaffEntry(int user_id, string modName, int roomid, string target, string type, string description)
        {
            using (IQueryAdapter queryreactor = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                queryreactor.SetQuery("INSERT INTO cmdlogs (user_id, user_name, roomid, command, extra_data, timestamp) VALUES (@userid,@username,@roomid,@type,@desc, UNIX_TIMESTAMP())");
                queryreactor.AddParameter("userid", user_id);
                queryreactor.AddParameter("username", modName);
                queryreactor.AddParameter("roomid", roomid);
                queryreactor.AddParameter("target", target);
                queryreactor.AddParameter("type", type);
                queryreactor.AddParameter("desc", description + " " + target);
                queryreactor.RunQuery();
            }
        }

        public static void PerformRoomAction(GameClient ModSession, int RoomId, bool KickUsers, bool LockRoom, bool InappropriateRoom)
        {
            Room room = ButterflyEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
            if (room == null)
            {
                return;
            }

            if (LockRoom)
            {
                room.RoomData.State = 1;
                room.RoomData.Name = "Cet appart ne respect par les conditions d'utilisation";
                using (IQueryAdapter queryreactor = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    queryreactor.RunQuery("UPDATE rooms SET state = 'locked' WHERE id = " + room.Id);
                }
            }
            if (InappropriateRoom)
            {
                room.RoomData.Name = ButterflyEnvironment.GetLanguageManager().TryGetValue("moderation.room.roomclosed", ModSession.Langue);
                room.RoomData.Description = ButterflyEnvironment.GetLanguageManager().TryGetValue("moderation.room.roomclosed", ModSession.Langue);
                room.ClearTags();
                room.RoomData.Tags.Clear();
                using (IQueryAdapter queryreactor = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    queryreactor.RunQuery("UPDATE rooms SET caption = 'Cet appart ne respect par les conditions dutilisation', description = 'Cet appart ne respect par les conditions dutilisation', tags = '' WHERE id = " + room.Id);
                }
            }
            if (KickUsers)
            {
                room.onRoomKick();
            }

            room.SendPacket(new GetGuestRoomResultComposer(ModSession, room.RoomData, false, false));
        }

        public static ServerPacket SerializeRoomTool(RoomData Data)
        {
            Room room = ButterflyEnvironment.GetGame().GetRoomManager().GetRoom(Data.Id);
            int i = 0;
            using (IQueryAdapter queryreactor = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                try
                {
                    queryreactor.SetQuery("SELECT id FROM users WHERE username = @owner");
                    queryreactor.AddParameter("owner", Data.OwnerName);
                    i = Convert.ToInt32(queryreactor.GetRow()[0]);
                }
                catch (Exception ex)
                {
                    Logging.HandleException(ex, "ModerationTool.SerializeRoomTool");
                }
            }
            ServerPacket serverMessage = new ServerPacket(ServerPacketHeader.MODTOOL_ROOM_INFO);
            serverMessage.WriteInteger(Data.Id);
            serverMessage.WriteInteger(Data.UsersNow);
            if (room != null)
            {
                serverMessage.WriteBoolean(room.GetRoomUserManager().GetRoomUserByName(Data.OwnerName) != null);
            }
            else
            {
                serverMessage.WriteBoolean(false);
            }

            serverMessage.WriteInteger(i);
            serverMessage.WriteString(Data.OwnerName);
            serverMessage.WriteBoolean(room != null);
            if (room != null)
            {
                serverMessage.WriteString(Data.Name);
                serverMessage.WriteString(Data.Description);
                serverMessage.WriteInteger(Data.TagCount);
                foreach (string s in Data.Tags)
                {
                    serverMessage.WriteString(s);
                }
            }
            return serverMessage;
        }

        public static void KickUser(GameClient ModSession, int UserId, string Message, bool Soft)
        {
            GameClient clientByUserId = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (clientByUserId == null || clientByUserId.GetHabbo().CurrentRoomId < 1 || clientByUserId.GetHabbo().Id == ModSession.GetHabbo().Id)
            {
                return;
            }

            if (clientByUserId.GetHabbo().Rank >= ModSession.GetHabbo().Rank)
            {
                ModSession.SendNotification(ButterflyEnvironment.GetLanguageManager().TryGetValue("moderation.kick.missingrank", ModSession.Langue));
            }
            else
            {
                Room room = ButterflyEnvironment.GetGame().GetRoomManager().GetRoom(clientByUserId.GetHabbo().CurrentRoomId);
                if (room == null)
                {
                    return;
                }

                room.GetRoomUserManager().RemoveUserFromRoom(clientByUserId, true, false);

                if (Soft)
                {
                    return;
                }

                if (ModSession.Antipub(Message, "<MT>"))
                {
                    return;
                }

                ButterflyEnvironment.GetGame().GetModerationManager().LogStaffEntry(ModSession.GetHabbo().Id, ModSession.GetHabbo().Username, 0, string.Empty, "Modtool", string.Format("Modtool kickalert: {0}", Message));

                clientByUserId.SendNotification(Message);
            }
        }

        public static void AlertUser(GameClient ModSession, int UserId, string Message, bool Caution)
        {
            GameClient clientByUserId = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (clientByUserId == null || clientByUserId.GetHabbo().Id == ModSession.GetHabbo().Id)
            {
                return;
            }

            if (Caution && clientByUserId.GetHabbo().Rank >= ModSession.GetHabbo().Rank)
            {
                ModSession.SendNotification(ButterflyEnvironment.GetLanguageManager().TryGetValue("moderation.caution.missingrank", ModSession.Langue));
                Caution = false;
            }

            if (ModSession.Antipub(Message, "<MT>"))
            {
                return;
            }

            ButterflyEnvironment.GetGame().GetModerationManager().LogStaffEntry(ModSession.GetHabbo().Id, ModSession.GetHabbo().Username, 0, string.Empty, "ModTool", string.Format("Modtool alert ( {1} ): {0}", Message, clientByUserId.GetHabbo().Username));

            clientByUserId.SendNotification(Message);
        }

        public static void BanUser(GameClient ModSession, int UserId, int Length, string Message)
        {
            GameClient clientByUserId = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (clientByUserId == null || clientByUserId.GetHabbo().Id == ModSession.GetHabbo().Id)
            {
                return;
            }

            if (clientByUserId.GetHabbo().Rank >= ModSession.GetHabbo().Rank)
            {
                ModSession.SendNotification(ButterflyEnvironment.GetLanguageManager().TryGetValue("moderation.ban.missingrank", ModSession.Langue));
            }
            else
            {
                double LengthSeconds = Length;
                ButterflyEnvironment.GetGame().GetClientManager().BanUser(clientByUserId, ModSession.GetHabbo().Username, LengthSeconds, Message, false, false);
            }
        }

        public static ServerPacket SerializeUserInfo(int UserId)
        {
            GameClient User = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            DataRow row1 = null;
            if (User == null)
            {
                using (IQueryAdapter queryreactor = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    queryreactor.SetQuery("SELECT id, username FROM users WHERE id = " + UserId);
                    row1 = queryreactor.GetRow();
                }
                if (row1 == null)
                {
                    return null;
                }
            }

            ServerPacket serverMessage = new ServerPacket(ServerPacketHeader.MODERATION_USER_INFO);
            serverMessage.WriteInteger((row1 == null) ? User.GetHabbo().Id : Convert.ToInt32(row1["id"]));
            serverMessage.WriteString((row1 == null) ? User.GetHabbo().Username : (string)row1["username"]);
            serverMessage.WriteString("Unknown");

            serverMessage.WriteInteger(0);
            serverMessage.WriteInteger(0);

            serverMessage.WriteBoolean(User != null);

            serverMessage.WriteInteger(0);
            serverMessage.WriteInteger(0);
            serverMessage.WriteInteger(0);
            serverMessage.WriteInteger(0);

            serverMessage.WriteInteger(0); // trading_lock_count_txt

            serverMessage.WriteString("");
            serverMessage.WriteString("");
            serverMessage.WriteInteger(0);
            serverMessage.WriteInteger(0);
            serverMessage.WriteString("Unknown");
            serverMessage.WriteString(""); // ???
            return serverMessage;
        }

        /*public static ServerMessage SerializeRoomVisits(int UserId)
        {
            ServerMessage serverMessage = new ServerMessage(537);
            serverMessage.WriteInteger(UserId);
            serverMessage.WriteString(ButterflyEnvironment.GetGame().GetClientManager().GetNameById(UserId));
            serverMessage.WriteInteger(0);
            return serverMessage;
        }*/

        public static ServerPacket SerializeUserChatlog(int UserId, int RoomId)
        {
            GameClient clientByUserId = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (clientByUserId == null || clientByUserId.GetHabbo() == null)
            {
                ServerPacket serverMessage = new ServerPacket(ServerPacketHeader.MODTOOL_USER_CHATLOG);
                serverMessage.WriteInteger(UserId);
                serverMessage.WriteString("User not online");
                serverMessage.WriteInteger(0);
                return serverMessage;
            }
            else
            {
                List<ChatMessage> sortedMessages = clientByUserId.GetHabbo().GetChatMessageManager().GetSortedMessages(0);
                ServerPacket packet = new ServerPacket(ServerPacketHeader.MODTOOL_USER_CHATLOG);
                packet.WriteInteger(UserId);
                packet.WriteString(clientByUserId.GetHabbo().Username);
                packet.WriteInteger(1);

                packet.WriteByte(1);
                packet.WriteShort(2);
                packet.WriteString("roomName");
                packet.WriteByte(2);
                packet.WriteString("RoomName"); // room name
                packet.WriteString("roomId");
                packet.WriteByte(1);
                packet.WriteInteger(RoomId);

                packet.WriteShort(sortedMessages.Count);
                foreach (ChatMessage chatMessage2 in sortedMessages)
                {
                    chatMessage2.Serialize(ref packet);
                }
                return packet;
            }
        }

        public static ServerPacket SerializeTicketChatlog(SupportTicket Ticket, RoomData RoomData, double Timestamp)
        {
            Room room = ButterflyEnvironment.GetGame().GetRoomManager().GetRoom(RoomData.Id);
            ServerPacket message = new ServerPacket(ServerPacketHeader.ModeratorTicketChatlogMessageComposer);
            message.WriteInteger(Ticket.TicketId);
            message.WriteInteger(Ticket.SenderId);
            message.WriteInteger(Ticket.ReportedId);
            message.WriteInteger(RoomData.Id);

            message.WriteBoolean(false);
            message.WriteInteger(RoomData.Id);
            message.WriteString(RoomData.Name);

            if (room == null)
            {
                message.WriteInteger(0);
                return message;
            }
            else
            {
                ChatMessageManager chatMessageManager = room.GetChatMessageManager();
                message.WriteInteger(chatMessageManager.messageCount);
                chatMessageManager.Serialize(ref message);
                return message;
            }
        }

        public static ServerPacket SerializeRoomChatlog(int roomID)
        {
            Room room = ButterflyEnvironment.GetGame().GetRoomManager().GetRoom(roomID);

            ServerPacket Message = new ServerPacket(ServerPacketHeader.MODTOOL_ROOM_CHATLOG);
            Message.WriteByte(1);

            Message.WriteShort(2);

            Message.WriteString("roomName");
            Message.WriteByte(2);
            Message.WriteString(room.RoomData.Name);

            Message.WriteString("roomId");
            Message.WriteByte(1);
            Message.WriteInteger(room.RoomData.Id);

            if (room == null)
            {
                Message.WriteShort(0);
                return Message;
            }
            else
            {
                ChatMessageManager chatMessageManager = room.GetChatMessageManager();
                Message.WriteShort(chatMessageManager.messageCount);
                chatMessageManager.Serialize(ref Message);
                return Message;
            }
        }
    }
}
