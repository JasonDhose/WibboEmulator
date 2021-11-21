using Butterfly.Communication.Packets.Outgoing.Rooms.Engine;
using Butterfly.Database.Daos;
using Butterfly.Database.Interfaces;
using Butterfly.Game.Clients;

namespace Butterfly.Game.Rooms.Chat.Commands.Cmd
{
    internal class DeleteMission : IChatCommand
    {
        public void Execute(Client Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            if (Params.Length != 2)
            {
                return;
            }

            string username = Params[1];
            Client clientByUsername = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            if (clientByUsername == null)
            {
                Session.SendNotification(ButterflyEnvironment.GetLanguageManager().TryGetValue("input.usernotfound", Session.Langue));
            }
            else if (Session.GetHabbo().Rank <= clientByUsername.GetHabbo().Rank)
            {
                Session.SendNotification(ButterflyEnvironment.GetLanguageManager().TryGetValue("user.notpermitted", Session.Langue));
            }
            else
            {
                clientByUsername.GetHabbo().Motto = ButterflyEnvironment.GetLanguageManager().TryGetValue("user.unacceptable_motto", clientByUsername.Langue);
                using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    UserDao.UpdateMotto(dbClient, clientByUsername.GetHabbo().Id, clientByUsername.GetHabbo().Motto);
                }

                Room currentRoom2 = clientByUsername.GetHabbo().CurrentRoom;
                if (currentRoom2 == null)
                {
                    return;
                }

                RoomUser roomUserByHabbo = currentRoom2.GetRoomUserManager().GetRoomUserByHabboId(clientByUsername.GetHabbo().Id);
                if (roomUserByHabbo == null)
                {
                    return;
                }

                currentRoom2.SendPacket(new UserChangeComposer(roomUserByHabbo, false));
            }

        }
    }
}
