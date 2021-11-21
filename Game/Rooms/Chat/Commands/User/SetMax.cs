using Butterfly.Game.Clients;

namespace Butterfly.Game.Rooms.Chat.Commands.Cmd
{
    internal class SetMax : IChatCommand
    {
        public void Execute(Client Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            int.TryParse(Params[1], out int MaxUsers);
            if (MaxUsers > 10000 || MaxUsers <= 0)
            {
                Room.SetMaxUsers(75);
            }
            else
            {
                Room.SetMaxUsers(MaxUsers);
            }
        }
    }
}
