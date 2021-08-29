using Butterfly.Communication.Packets.Outgoing.Rooms.Chat;
using Butterfly.HabboHotel.GameClients;using Butterfly.HabboHotel.Users;

namespace Butterfly.HabboHotel.Rooms.Chat.Commands.Cmd{    internal class UnMute : IChatCommand    {        public void Execute(GameClient Session, Room Room, RoomUser UserRoom, string[] Params)        {            GameClient clientByUsername = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);            if (clientByUsername == null || clientByUsername.GetHabbo() == null)            {                Session.SendNotification(ButterflyEnvironment.GetLanguageManager().TryGetValue("input.usernotfound", Session.Langue));            }            else            {                Habbo habboByUsername = clientByUsername.GetHabbo();

                habboByUsername.spamProtectionTime = 10;                habboByUsername.spamEnable = true;            }        }    }}