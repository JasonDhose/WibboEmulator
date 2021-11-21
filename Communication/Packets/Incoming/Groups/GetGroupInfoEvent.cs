﻿using Butterfly.Communication.Packets.Outgoing.Groups;
using Butterfly.Game.GameClients;
using Butterfly.Game.Guilds;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class GetGroupInfoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            bool NewWindow = Packet.PopBoolean();

            if (!ButterflyEnvironment.GetGame().GetGroupManager().TryGetGroup(GroupId, out Guild Group))
            {
                return;
            }

            Session.SendPacket(new GroupInfoComposer(Group, Session, NewWindow));
        }
    }
}
