namespace WibboEmulator.Database.Daos.Item;
using System.Data;
using WibboEmulator.Database.Interfaces;

internal sealed class ItemMoodlightDao
{
    internal static DataRow GetOne(IQueryAdapter dbClient, int itemId)
    {
        dbClient.SetQuery("SELECT enabled, current_preset, preset_one, preset_two, preset_three FROM `item_moodlight` WHERE item_id = '" + itemId + "' LIMIT 1");
        return dbClient.GetRow();
    }

    internal static string UpdateEnableString(int itemId, int enabled) => "UPDATE `item_moodlight` SET enabled = '" + enabled + "' WHERE item_id = '" + itemId + "' LIMIT 1";

    internal static void Update(IQueryAdapter dbClient, int itemId, string color, string pr, int intensity, bool bgOnly)
    {
        dbClient.SetQuery("UPDATE `item_moodlight` SET `preset_" + pr + "` = '@color," + intensity + "," + (bgOnly ? "1" : "0") + "' WHERE item_id = '" + itemId + "' LIMIT 1");
        dbClient.AddParameter("color", color);
        dbClient.RunQuery();
    }

    internal static void Insert(IQueryAdapter dbClient, int itemId)
    {
        dbClient.SetQuery("INSERT INTO `item_moodlight` (item_id, enabled, current_preset, preset_one, preset_two, preset_three) VALUES (@id, '0', 1, @preset, @preset, @preset)");
        dbClient.AddParameter("id", itemId);
        dbClient.AddParameter("preset", "#000000,255,0");
        dbClient.RunQuery();
    }

    internal static void InsertDuplicate(IQueryAdapter dbClient, int itemId, int oldItemId) => dbClient.RunQuery("INSERT INTO `item_moodlight` (item_id, enabled, current_preset, preset_one, preset_two, preset_three)" +
            "SELECT '" + itemId + "', enabled, current_preset, preset_one, preset_two, preset_three FROM `item_moodlight` WHERE item_id = '" + oldItemId + "'");
}