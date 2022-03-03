using Butterfly.Communication.Packets.Outgoing.Rooms.Engine;
using Butterfly.Game.Clients;
using Butterfly.Game.Rooms.Games;
using Butterfly.Game.Rooms;

namespace Butterfly.Game.Chat.Commands.Cmd
{
    internal class TransfBot : IChatCommand
    {
        public void Execute(Client Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            if (UserRoom.Team != TeamType.NONE || UserRoom.InGame)
            {
                return;
            }

            if (!UserRoom.transformation && !UserRoom.IsSpectator)
            {
                Room RoomClient = Session.GetUser().CurrentRoom;
                if (RoomClient != null)
                {
                    UserRoom.transfbot = !UserRoom.transfbot;

                    RoomClient.SendPacket(new UserRemoveComposer(UserRoom.VirtualId));
                    RoomClient.SendPacket(new UsersComposer(UserRoom));
                }
            }
        }
    }
}
