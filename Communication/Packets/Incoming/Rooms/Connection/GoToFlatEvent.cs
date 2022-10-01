﻿using WibboEmulator.Communication.Packets.Outgoing.Rooms.Session;
using WibboEmulator.Games.GameClients;

namespace WibboEmulator.Communication.Packets.Incoming.Structure
{
    internal class GoToFlatEvent : IPacketEvent
    {
        public double Delay => 0;

        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetUser().InRoom)
            {
                return;
            }

            if (!Session.GetUser().EnterRoom(Session.GetUser().CurrentRoom))
            {
                Session.SendPacket(new CloseConnectionComposer());
            }
        }
    }
}
