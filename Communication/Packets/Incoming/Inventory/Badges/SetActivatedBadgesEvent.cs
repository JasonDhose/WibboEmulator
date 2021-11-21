using Butterfly.Communication.Packets.Outgoing;
using Butterfly.Database.Daos;
using Butterfly.Database.Interfaces;
using Butterfly.Game.GameClients;
using Butterfly.Game.Quests;
using Butterfly.Game.User.Badges;

namespace Butterfly.Communication.Packets.Incoming.Structure
{
    internal class SetActivatedBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.GetHabbo().GetBadgeComponent().ResetSlots();

            using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                UserBadgeDao.UpdateResetSlot(dbClient, Session.GetHabbo().Id);
            }

            for (int i = 0; i < 5; i++)
            {
                int Slot = Packet.PopInt();
                string Badge = Packet.PopString();

                if (string.IsNullOrEmpty(Badge))
                {
                    continue;
                }

                if (!Session.GetHabbo().GetBadgeComponent().HasBadge(Badge) || Slot < 1 || Slot > 5)
                {
                    continue;
                }

                Session.GetHabbo().GetBadgeComponent().GetBadge(Badge).Slot = Slot;

                using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    UserBadgeDao.UpdateSlot(dbClient, Session.GetHabbo().Id, Slot, Badge);
                }
            }

            ButterflyEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.PROFILE_BADGE, 0);

            ServerPacket Message = new ServerPacket(ServerPacketHeader.USER_BADGES_CURRENT);
            Message.WriteInteger(Session.GetHabbo().Id);
            Message.WriteInteger(Session.GetHabbo().GetBadgeComponent().EquippedCount);

            int BadgeCount = 0;
            foreach (Badge badge in Session.GetHabbo().GetBadgeComponent().BadgeList.Values)
            {
                if (badge.Slot > 0)
                {
                    BadgeCount++;
                    if (BadgeCount > 5)
                    {
                        break;
                    }

                    Message.WriteInteger(badge.Slot);
                    Message.WriteString(badge.Code);
                }
            }

            if (Session.GetHabbo().InRoom && ButterflyEnvironment.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId) != null)
            {
                ButterflyEnvironment.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId).SendPacket(Message);
            }

            Session.SendPacket(Message);
        }
    }
}
