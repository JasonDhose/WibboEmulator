using Butterfly.Game.Clients;
using Butterfly.Game.Rooms;

namespace Butterfly.Game.Chat.Commands.Cmd
{
    internal class DisableFriendRequests : IChatCommand
    {
        public void Execute(Client Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            if (Session.GetUser().HasFriendRequestsDisabled)
            {
                Session.GetUser().HasFriendRequestsDisabled = false;
                UserRoom.SendWhisperChat(ButterflyEnvironment.GetLanguageManager().TryGetValue("cmd.textamigo.true", Session.Langue));
            }
            else
            {
                Session.GetUser().HasFriendRequestsDisabled = true;
                UserRoom.SendWhisperChat(ButterflyEnvironment.GetLanguageManager().TryGetValue("cmd.textamigo.false", Session.Langue));
            }

        }
    }
}
