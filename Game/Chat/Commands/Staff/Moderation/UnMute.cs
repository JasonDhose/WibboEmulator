using Butterfly.Game.Clients;
using Butterfly.Game.Users;
using Butterfly.Game.Rooms;

namespace Butterfly.Game.Chat.Commands.Cmd
{
    internal class UnMute : IChatCommand
    {
        public void Execute(Client Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            Client TargetUser = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetUser == null || TargetUser.GetUser() == null)
            {
                Session.SendNotification(ButterflyEnvironment.GetLanguageManager().TryGetValue("input.usernotfound", Session.Langue));
            }
            else
            {
                User user = TargetUser.GetUser();

                user.SpamProtectionTime = 10;
                user.SpamEnable = true;
            }
        }
    }
}
