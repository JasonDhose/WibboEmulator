using Butterfly.Communication.Packets.Outgoing;
using Butterfly.Communication.Packets.Outgoing.Avatar;
using Butterfly.Communication.Packets.Outgoing.BuildersClub;
using Butterfly.Communication.Packets.Outgoing.Camera;
using Butterfly.Communication.Packets.Outgoing.Catalog;
using Butterfly.Communication.Packets.Outgoing.GameCenter;
using Butterfly.Communication.Packets.Outgoing.Groups;
using Butterfly.Communication.Packets.Outgoing.Handshake;
using Butterfly.Communication.Packets.Outgoing.Help;
using Butterfly.Communication.Packets.Outgoing.Inventory;
using Butterfly.Communication.Packets.Outgoing.Inventory.Achievements;
using Butterfly.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Butterfly.Communication.Packets.Outgoing.Inventory.Badges;
using Butterfly.Communication.Packets.Outgoing.Inventory.Bots;
using Butterfly.Communication.Packets.Outgoing.Inventory.Furni;
using Butterfly.Communication.Packets.Outgoing.Inventory.Pets;
using Butterfly.Communication.Packets.Outgoing.Inventory.Purse;
using Butterfly.Communication.Packets.Outgoing.Inventory.Trading;
using Butterfly.Communication.Packets.Outgoing.LandingView;
using Butterfly.Communication.Packets.Outgoing.MarketPlace;
using Butterfly.Communication.Packets.Outgoing.Messenger;
using Butterfly.Communication.Packets.Outgoing.Misc;
using Butterfly.Communication.Packets.Outgoing.Moderation;
using Butterfly.Communication.Packets.Outgoing.Navigator;
using Butterfly.Communication.Packets.Outgoing.Notifications;
using Butterfly.Communication.Packets.Outgoing.Pets;
using Butterfly.Communication.Packets.Outgoing.Quests;
using Butterfly.Communication.Packets.Outgoing.Rooms;
using Butterfly.Communication.Packets.Outgoing.Rooms.Action;
using Butterfly.Communication.Packets.Outgoing.Rooms.AI.Bots;
using Butterfly.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Butterfly.Communication.Packets.Outgoing.Rooms.Avatar;
using Butterfly.Communication.Packets.Outgoing.Rooms.Chat;
using Butterfly.Communication.Packets.Outgoing.Rooms.Engine;
using Butterfly.Communication.Packets.Outgoing.Rooms.FloorPlan;
using Butterfly.Communication.Packets.Outgoing.Rooms.Freeze;
using Butterfly.Communication.Packets.Outgoing.Rooms.Furni;
using Butterfly.Communication.Packets.Outgoing.Rooms.Furni.Furni;
using Butterfly.Communication.Packets.Outgoing.Rooms.Furni.Moodlight;
using Butterfly.Communication.Packets.Outgoing.Rooms.Furni.Stickys;
using Butterfly.Communication.Packets.Outgoing.Rooms.Furni.Wired;
using Butterfly.Communication.Packets.Outgoing.Rooms.Notifications;
using Butterfly.Communication.Packets.Outgoing.Rooms.Permissions;
using Butterfly.Communication.Packets.Outgoing.Rooms.Session;
using Butterfly.Communication.Packets.Outgoing.Rooms.Settings;
using Butterfly.Communication.Packets.Outgoing.Rooms.Wireds;
using Butterfly.Communication.Packets.Outgoing.Sound;
using Butterfly.Communication.Packets.Outgoing.Users;
using Butterfly.Communication.Packets.Outgoing.WebSocket;

using Butterfly.HabboHotel.GameClients;
using System;

namespace Butterfly.HabboHotel.Rooms.Chat.Commands.Cmd
{
    internal class NoFace : IChatCommand
    {
        public void Execute(GameClient Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            if (UserRoom.transformation || UserRoom.IsSpectator)
            {
                return;
            }

            string look = Session.GetHabbo().Look;

            if (look.Contains("hd-"))
            {
                string hdlook = look.Split(new string[] { "hd-" }, StringSplitOptions.None)[1];
                string hdcode = "hd-" + hdlook.Split(new char[] { '.' })[0]; //ex : hd-180-22
                string hdcodecolor = "";
                if (hdcode.Split('-').Length == 3)
                {
                    hdcodecolor = hdcode.Split('-')[2];
                }

                string hdcodenoface = "hd-99999-" + hdcodecolor; //hd-9999-22

                look = look.Replace(hdcode, hdcodenoface);

                Session.GetHabbo().Look = look;

                if (!Session.GetHabbo().InRoom)
                {
                    return;
                }

                Room currentRoom = Session.GetHabbo().CurrentRoom;
                if (currentRoom == null)
                {
                    return;
                }

                currentRoom.SendPacket(new UserChangeComposer(UserRoom, false));

            }

        }
    }
}
