namespace WibboEmulator.Database.Daos.User;
using System.Data;
using WibboEmulator.Database.Interfaces;
using WibboEmulator.Utilities;

internal sealed class UserBadgeDao
{
    internal static void UpdateResetSlot(IQueryAdapter dbClient, int userId) => dbClient.RunQuery("UPDATE `user_badge` SET badge_slot = '0' WHERE user_id = '" + userId + "' AND badge_slot != '0'");

    internal static void UpdateSlot(IQueryAdapter dbClient, int userId, int slot, string badge)
    {
        dbClient.SetQuery("UPDATE `user_badge` SET badge_slot = '" + slot + "' WHERE badge_id = @badge AND user_id = '" + userId + "'");
        dbClient.AddParameter("badge", badge);
        dbClient.RunQuery();
    }

    internal static void InsertAll(IQueryAdapter dbClient, List<int> userIds, string badge)
    {
        if (userIds.Count == 0)
        {
            return;
        }

        var standardQueries = new QueryChunk();

        foreach (var userId in userIds)
        {
            standardQueries.AddQuery("INSERT INTO `user_badge` (user_id,badge_id,badge_slot) VALUES (" + userId + ",@badge,0)");
        }
        standardQueries.AddParameter("badge", badge);

        standardQueries.Execute(dbClient);
        standardQueries.Dispose();
    }

    internal static void Insert(IQueryAdapter dbClient, int userId, int slot, string badge)
    {
        dbClient.SetQuery("INSERT INTO `user_badge` (user_id,badge_id,badge_slot) VALUES (" + userId + ",@badge," + slot + ")");
        dbClient.AddParameter("badge", badge);
        dbClient.RunQuery();
    }

    internal static void Delete(IQueryAdapter dbClient, int userId, string badge)
    {
        dbClient.SetQuery("DELETE FROM `user_badge` WHERE badge_id = @badge AND user_id = '" + userId + "' LIMIT 1");
        dbClient.AddParameter("badge", badge);
        dbClient.RunQuery();
    }

    internal static DataTable GetAll(IQueryAdapter dbClient, int userId)
    {
        dbClient.SetQuery("SELECT `user_id`, `badge_id`, `badge_slot` FROM `user_badge` WHERE user_id = '" + userId + "'");
        return dbClient.GetTable();
    }
}