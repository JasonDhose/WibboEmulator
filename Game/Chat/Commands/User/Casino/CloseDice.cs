using WibboEmulator.Game.Clients;
using WibboEmulator.Game.Items;
using WibboEmulator.Game.Rooms;

namespace WibboEmulator.Game.Chat.Commands.Cmd
{
    internal class CloseDice : IChatCommand
    {
        public void Execute(Client Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            List<Item> userBooth = Room.GetRoomItemHandler().GetFloor.Where(x => x != null && Gamemap.TilesTouching(
                x.X, x.Y, UserRoom.X, UserRoom.Y) && x.Data.InteractionType == InteractionType.DICE).ToList();

            if (userBooth == null)
            {
                return;
            }

            UserRoom.DiceCounterAmount = 0;
            UserRoom.DiceCounter = 0;

            userBooth.ForEach(x =>
            {
                x.ExtraData = "0";
                x.UpdateState();
            });
        }
    }
}
