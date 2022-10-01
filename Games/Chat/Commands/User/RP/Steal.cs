﻿using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Roleplay.Player;
using WibboEmulator.Games.Rooms;

namespace WibboEmulator.Games.Chat.Commands.Cmd
{
    internal class Steal : IChatCommand
    {
        public void Execute(GameClient Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            if (Params.Length != 3)
            {
                return;
            }

            if (!Room.IsRoleplay)
            {
                return;
            }

            RolePlayer Rp = UserRoom.Roleplayer;
            if (Rp == null)
            {
                return;
            }

            if (Rp.Dead || Rp.SendPrison)
            {
                return;
            }

            RoomUser TargetRoomUser = Room.GetRoomUserManager().GetRoomUserByName(Params[1].ToString());

            if (TargetRoomUser == null || TargetRoomUser.GetClient() == null || TargetRoomUser.GetClient().GetUser() == null)
            {
                return;
            }

            RolePlayer RpTwo = TargetRoomUser.Roleplayer;
            if (RpTwo == null)
            {
                return;
            }

            if (TargetRoomUser.GetClient().GetUser().Id == Session.GetUser().Id)
            {
                return;
            }

            if (RpTwo.Dead || RpTwo.SendPrison)
            {
                return;
            }

            int NumberMoney = (int)Math.Floor((double)((double)Rp.Money / 100) * 15);

            if (Rp.Money < NumberMoney)
            {
                return;
            }

            if (!((Math.Abs((TargetRoomUser.X - UserRoom.X)) >= 2) || (Math.Abs((TargetRoomUser.Y - UserRoom.Y)) >= 2)))
            {
                Rp.Money += NumberMoney;
                RpTwo.Money -= NumberMoney;

                Rp.SendUpdate();
                RpTwo.SendUpdate();

                TargetRoomUser.SendWhisperChat(string.Format(WibboEnvironment.GetLanguageManager().TryGetValue("rp.vole.receive", TargetRoomUser.GetClient().Langue), NumberMoney, UserRoom.GetUsername()));

                Session.SendWhisper(string.Format(WibboEnvironment.GetLanguageManager().TryGetValue("rp.vole.send", Session.Langue), NumberMoney, TargetRoomUser.GetUsername()));
                UserRoom.OnChat(string.Format(WibboEnvironment.GetLanguageManager().TryGetValue("rp.vole.send.chat", Session.Langue), TargetRoomUser.GetUsername()), 0, true);
            }
        }
    }
}
