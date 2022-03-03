using Butterfly.Communication.Packets.Outgoing.Rooms.Permissions;
using Butterfly.Communication.Packets.Outgoing.Rooms.Settings;
using Butterfly.Database.Daos;
using Butterfly.Database.Interfaces;
using Butterfly.Game.Clients;
using Butterfly.Game.Rooms;
using Butterfly.Game.Users;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class AssignRightsEvent : IPacketEvent
    {
        public double Delay => 250;

        public void Parse(Client Session, ClientPacket Packet)
        {
            if (Session.GetUser() == null)
            {
                return;
            }

            int UserId = Packet.PopInt();

            Room room = ButterflyEnvironment.GetGame().GetRoomManager().GetRoom(Session.GetUser().CurrentRoomId);
            if (room == null)
            {
                return;
            }

            if (room == null || !room.CheckRights(Session, true))
            {
                return;
            }

            if (room.UsersWithRights.Contains(UserId))
            {
                Session.SendNotification(ButterflyEnvironment.GetLanguageManager().TryGetValue("user.giverights.error", Session.Langue));
            }
            else
            {
                User Userright = ButterflyEnvironment.GetUserById(UserId);
                if (Userright == null)
                {
                    return;
                }

                room.UsersWithRights.Add(UserId);

                using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    RoomRightDao.Insert(dbClient, room.Id, UserId);
                }

                Session.SendPacket(new FlatControllerAddedComposer(room.Id, UserId, Userright.Username));

                RoomUser roomUserByUserId = room.GetRoomUserManager().GetRoomUserByUserId(UserId);
                if (roomUserByUserId == null || roomUserByUserId.IsBot)
                {
                    return;
                }

                roomUserByUserId.RemoveStatus("flatctrl 0");
                roomUserByUserId.SetStatus("flatctrl 1", "");
                roomUserByUserId.UpdateNeeded = true;

                roomUserByUserId.GetClient().SendPacket(new YouAreControllerComposer(1));
            }
        }
    }
}