namespace WibboEmulator.Communication.Packets.Incoming.Rooms.Avatar;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Engine;
using WibboEmulator.Database.Daos.User;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Quests;

internal sealed class ChangeMottoEvent : IPacketEvent
{
    public double Delay => 250;

    public void Parse(GameClient session, ClientPacket packet)
    {
        var newMotto = packet.PopString(38);
        if (newMotto == session.User.Motto)
        {
            return;
        }

        if (!session.User.HasPermission("word_filter_override"))
        {
            newMotto = WibboEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(newMotto);
        }

        if (session.User.IgnoreAll)
        {
            return;
        }

        if (session.User.CheckChatMessage(newMotto, "<MOTTO>"))
        {
            return;
        }

        session.User.Motto = newMotto;

        using (var dbClient = WibboEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            UserDao.UpdateMotto(dbClient, session.User.Id, newMotto);
        }

        WibboEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.ProfileChangeMotto, 0);

        if (session.User.InRoom)
        {
            var currentRoom = session.User.CurrentRoom;
            if (currentRoom == null)
            {
                return;
            }

            var roomUserByUserId = currentRoom.RoomUserManager.GetRoomUserByUserId(session.User.Id);
            if (roomUserByUserId == null)
            {
                return;
            }

            if (roomUserByUserId.IsTransf || roomUserByUserId.IsSpectator)
            {
                return;
            }

            currentRoom.SendPacket(new UserChangeComposer(roomUserByUserId, false));
        }

        _ = WibboEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_Motto", 1);
    }
}
