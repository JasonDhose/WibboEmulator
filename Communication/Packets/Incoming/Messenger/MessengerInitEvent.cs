﻿using Butterfly.Communication.Packets.Outgoing.Messenger;
using Butterfly.Game.Clients;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class MessengerInitEvent : IPacketEvent
    {
        public double Delay => 0;

        public void Parse(Client Session, ClientPacket Packet)
        {
            Session.GetUser().GetMessenger().OnStatusChanged();

            Session.SendPacket(new MessengerInitComposer());
            Session.SendPacket(new BuddyListComposer(Session.GetUser().GetMessenger().Friends));
            Session.GetUser().GetMessenger().ProcessOfflineMessages();
        }
    }
}
