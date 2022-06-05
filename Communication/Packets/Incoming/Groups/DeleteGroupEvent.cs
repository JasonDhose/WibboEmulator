﻿using Wibbo.Database.Daos;
using Wibbo.Database.Interfaces;
using Wibbo.Game.Clients;
using Wibbo.Game.Groups;
using Wibbo.Game.Rooms;

namespace Wibbo.Communication.Packets.Incoming.Structure
{
    internal class DeleteGroupEvent : IPacketEvent
    {
        public double Delay => 1000;

        public void Parse(Client Session, ClientPacket Packet)
        {
            int groupId = Packet.PopInt();

            if (!WibboEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group Group))
            {
                return;
            }

            if (Group.CreatorId != Session.GetUser().Id && !Session.GetUser().HasFuse("group_delete_override"))
            {
                Session.SendNotification(WibboEnvironment.GetLanguageManager().TryGetValue("notif.groupdelete.error.1", Session.Langue));
                return;
            }

            if (Group.MemberCount >= 100 && !Session.GetUser().HasFuse("group_delete_limit_override"))
            {
                Session.SendNotification(WibboEnvironment.GetLanguageManager().TryGetValue("notif.groupdelete.error.2", Session.Langue));
                return;
            }

            Room Room = WibboEnvironment.GetGame().GetRoomManager().LoadRoom(Group.RoomId);

            if (Room != null)
            {
                Room.RoomData.Group = null;
            }

            WibboEnvironment.GetGame().GetGroupManager().DeleteGroup(Group.Id);

            using (IQueryAdapter dbClient = WibboEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                GuildDao.Delete(dbClient, Group.Id);
                GuildMembershipDao.Delete(dbClient, Group.Id);
                GuildRequestDao.Delete(dbClient, Group.Id);
                RoomDao.UpdateResetGroupId(dbClient, Group.RoomId);
                UserStatsDao.UpdateRemoveAllGroupId(dbClient, Group.Id);

                if (Group.CreatorId != Session.GetUser().Id)
                {
                    LogStaffDao.Insert(dbClient, Session.GetUser().Username, $"Suppresion du groupe {Group.Id} crée par {Group.CreatorId}");
                }
            }

            WibboEnvironment.GetGame().GetRoomManager().UnloadRoom(Room);

            Session.SendNotification(WibboEnvironment.GetLanguageManager().TryGetValue("notif.groupdelete.succes", Session.Langue));
        }
    }
}
