﻿using Wibbo.Communication.Packets.Outgoing.Rooms.Session;
using Wibbo.Game.Clients;

namespace Wibbo.Communication.Packets.Incoming.Structure
{
    internal class GoToFlatEvent : IPacketEvent
    {
        public double Delay => 0;

        public void Parse(Client Session, ClientPacket Packet)
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
