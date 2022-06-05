using Wibbo.Game.Clients;
using Wibbo.Game.Rooms;

namespace Wibbo.Game.Chat.Commands.Cmd
{
    internal class DisableFriendRequests : IChatCommand
    {
        public void Execute(Client Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            if (Session.GetUser().HasFriendRequestsDisabled)
            {
                Session.GetUser().HasFriendRequestsDisabled = false;
                Session.SendWhisper(WibboEnvironment.GetLanguageManager().TryGetValue("cmd.textamigo.true", Session.Langue));
            }
            else
            {
                Session.GetUser().HasFriendRequestsDisabled = true;
                Session.SendWhisper(WibboEnvironment.GetLanguageManager().TryGetValue("cmd.textamigo.false", Session.Langue));
            }

        }
    }
}
