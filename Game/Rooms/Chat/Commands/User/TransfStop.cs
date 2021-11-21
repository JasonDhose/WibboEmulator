using Butterfly.Communication.Packets.Outgoing.Rooms.Engine;

using Butterfly.Game.Clients;
using Butterfly.Game.Rooms.Games;

namespace Butterfly.Game.Rooms.Chat.Commands.Cmd
{
    internal class TransfStop : IChatCommand
    {
        public void Execute(Client Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            if (UserRoom.Team != Team.none || UserRoom.InGame)
            {
                return;
            }

            if (UserRoom.transformation && !UserRoom.IsSpectator && !UserRoom.InGame)
            {
                Room RoomClient = Session.GetHabbo().CurrentRoom;
                if (RoomClient != null)
                {
                    UserRoom.transformation = false;

                    RoomClient.SendPacket(new UserRemoveComposer(UserRoom.VirtualId));
                    RoomClient.SendPacket(new UsersComposer(UserRoom));
                }
            }

        }
    }
}
