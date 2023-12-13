namespace WibboEmulator.Database.Daos.Room;
using System.Data;
using WibboEmulator.Database.Interfaces;

internal sealed class RoomDao
{
    internal static void UpdateResetGroupId(IQueryAdapter dbClient, int id) => dbClient.RunQuery("UPDATE `room` SET group_id = '0' WHERE id = '" + id + "' LIMIT 1");

    internal static void UpdateGroupId(IQueryAdapter dbClient, int groupId, int roomId)
    {
        dbClient.SetQuery("UPDATE `room` SET group_id = @gid WHERE id = @rid LIMIT 1");
        dbClient.AddParameter("gid", groupId);
        dbClient.AddParameter("rid", roomId);
        dbClient.RunQuery();
    }


    internal static void UpdateWiredSecurity(IQueryAdapter dbClient, int roomId, bool enabled)
    {
        dbClient.SetQuery("UPDATE `room` SET wired_security = @enabled WHERE id = @id LIMIT 1");
        dbClient.AddParameter("enabled", enabled ? 1 : 0);
        dbClient.AddParameter("id", roomId);
        dbClient.RunQuery();
    }

    internal static void UpdateScore(IQueryAdapter dbClient, int roomId, int score) => dbClient.RunQuery("UPDATE `room` SET score = '" + score + "' WHERE id = '" + roomId + "'");

    internal static void UpdateDecoration(IQueryAdapter dbClient, int roomId, string decorationKey, string extraData)
    {
        dbClient.SetQuery("UPDATE `room` SET `" + decorationKey + "` = @extradata WHERE id = '" + roomId + "' LIMIT 1");
        dbClient.AddParameter("extradata", extraData);
        dbClient.RunQuery();
    }

    internal static void UpdateModelWallThickFloorThick(IQueryAdapter dbClient, int roomId, int wallThick, int floorThick) => dbClient.RunQuery("UPDATE `room` SET model_name = 'model_custom', wallthick = '" + wallThick + "', floorthick = '" + floorThick + "' WHERE id = " + roomId + " LIMIT 1");

    internal static void Delete(IQueryAdapter dbClient, int roomId) => dbClient.RunQuery("DELETE FROM `room` WHERE id = '" + roomId + "'");

    internal static void UpdateAll(IQueryAdapter dbClient, int roomId, string name, string description, string password, string tags, int categoryId, string state, int maxUsers, bool allowPets, bool allowPetsEat, bool allowWalkthrough, bool hidewall, int floorThickness, int wallThickness, int mutefuse, int kickfuse, int banfuse, int chatType, int chatBalloon, int chatSpeed, int chatMaxDistance, int chatFloodProtection, int trocStatus)
    {
        dbClient.SetQuery("UPDATE `room` SET caption = @caption, description = @description, password = @password, category = '" + categoryId + "', state = '" + state + "', tags = @tags, users_max = '" + maxUsers + "', allow_pets = '" + (allowPets ? 1 : 0) + "', allow_pets_eat = '" + (allowPetsEat ? 1 : 0) + "', allow_walkthrough = '" + (allowWalkthrough ? 1 : 0) + "', allow_hidewall = '" + (hidewall ? 1 : 0) + "', floorthick = '" + floorThickness + "', wallthick = '" + wallThickness + "', moderation_mute_fuse = '" + mutefuse + "', moderation_kick_fuse = '" + kickfuse + "', moderation_ban_fuse = '" + banfuse + "', chat_type = '" + chatType + "', chat_balloon = '" + chatBalloon + "', chat_speed = '" + chatSpeed + "', chat_max_distance = '" + chatMaxDistance + "', chat_flood_protection = '" + chatFloodProtection + "', troc_status = '" + trocStatus + "' WHERE id = '" + roomId + "'");
        dbClient.AddParameter("caption", name);
        dbClient.AddParameter("description", description);
        dbClient.AddParameter("password", password);
        dbClient.AddParameter("tags", tags);
        dbClient.RunQuery();
    }

    internal static void UpdateOwner(IQueryAdapter dbClient, string newUsername, string username)
    {
        dbClient.SetQuery("UPDATE `room` SET owner = @newname WHERE owner = @oldname");
        dbClient.AddParameter("newname", newUsername);
        dbClient.AddParameter("oldname", username);
        dbClient.RunQuery();
    }

    internal static void UpdateOwnerByRoomId(IQueryAdapter dbClient, string newUsername, int roomId)
    {
        dbClient.SetQuery("UPDATE `room` SET owner = @newname WHERE id = '" + roomId + "'");
        dbClient.AddParameter("newname", newUsername);
        dbClient.RunQuery();
    }

    internal static DataTable GetAllIdByOwner(IQueryAdapter dbClient, string username)
    {
        dbClient.SetQuery("SELECT id FROM `room` WHERE owner = @name");
        dbClient.AddParameter("name", username);

        return dbClient.GetTable();
    }


    internal static void UpdateResetUsersNow(IQueryAdapter dbClient) => dbClient.RunQuery("UPDATE `room` SET users_now = '0' WHERE users_now > '0'");

    internal static int InsertDuplicate(IQueryAdapter dbClient, string username, string desc)
    {
        dbClient.SetQuery("INSERT INTO `room` (caption,description,owner,model_name,category,state, wallpaper, floor, landscape, allow_hidewall, wallthick, floorthick) SELECT @caption, @desc, @username, @model, category, state, wallpaper, floor, landscape, allow_hidewall, wallthick, floorthick FROM `room` WHERE id = '5328079'");
        dbClient.AddParameter("caption", username);
        dbClient.AddParameter("desc", desc);
        dbClient.AddParameter("username", username);
        dbClient.AddParameter("model", "model_welcome");
        return Convert.ToInt32(dbClient.InsertQuery());
    }

    internal static DataTable GetAllSearchByUsername(IQueryAdapter dbClient, string searchData)
    {
        dbClient.SetQuery("SELECT room.`id`, room.`caption`, room.`owner`, room.`description`, room.`category`, room.`state`, room.`users_max`, room.`model_name`, room.`score`, room.`tags`, room.`password`, room.`wallpaper`, room.`floor`, room.`landscape`, room.`allow_pets`, room.`allow_pets_eat`, room.`allow_walkthrough`, room.`allow_hidewall`, room.`wallthick`, room.`floorthick`, room.`moderation_mute_fuse`, room.`allow_rightsoverride`, room.`moderation_kick_fuse`, room.`moderation_ban_fuse`, room.`group_id`, room.`chat_type`, room.`chat_balloon`, room.`chat_speed`, room.`chat_max_distance`, room.`chat_flood_protection`, room.`troc_status`, room.`users_now`, room.`allow_hidewireds`, room.`price`, room.`wired_security`, user.`id` as `owner_id`, user.`langue` FROM `room` LEFT JOIN user ON room.owner = user.username WHERE room.owner = @username AND room.state != 'invisible' ORDER BY room.users_now DESC");
        dbClient.AddParameter("username", searchData);
        return dbClient.GetTable();
    }

    internal static DataTable GetAllSearch(IQueryAdapter dbClient, string searchData)
    {
        dbClient.SetQuery("SELECT room.`id`, room.`caption`, room.`owner`, room.`description`, room.`category`, room.`state`, room.`users_max`, room.`model_name`, room.`score`, room.`tags`, room.`password`, room.`wallpaper`, room.`floor`, room.`landscape`, room.`allow_pets`, room.`allow_pets_eat`, room.`allow_walkthrough`, room.`allow_hidewall`, room.`wallthick`, room.`floorthick`, room.`moderation_mute_fuse`, room.`allow_rightsoverride`, room.`moderation_kick_fuse`, room.`moderation_ban_fuse`, room.`group_id`, room.`chat_type`, room.`chat_balloon`, room.`chat_speed`, room.`chat_max_distance`, room.`chat_flood_protection`, room.`troc_status`, room.`users_now`, room.`allow_hidewireds`, room.`price`, room.`wired_security`, user.`id` as `owner_id`, user.`langue` FROM `room` LEFT JOIN user ON room.owner = user.username WHERE room.caption LIKE @query OR room.owner LIKE @query ORDER BY room.users_now DESC LIMIT 50");
        dbClient.AddParameter("query", searchData.Replace("%", "\\%").Replace("_", "\\_") + "%");
        return dbClient.GetTable();
    }

    internal static int InsertDuplicate(IQueryAdapter dbClient, int roomId, string username)
    {
        dbClient.SetQuery("INSERT INTO `room` (caption, owner, description, model_name, wallpaper, floor, landscape, allow_hidewall, wallthick, floorthick, allow_rightsoverride, allow_hidewireds)" +
        "SELECT 'Appart " + roomId + " copie', '" + username + "', description, model_name, wallpaper, floor, landscape, allow_hidewall, wallthick, floorthick, allow_rightsoverride, allow_hidewireds FROM `room` WHERE id = '" + roomId + "'; ");
        return Convert.ToInt32(dbClient.InsertQuery());
    }

    internal static void UpdateModel(IQueryAdapter dbClient, int roomId) => dbClient.RunQuery("UPDATE `room` SET model_name = 'model_custom' WHERE id = '" + roomId + "' LIMIT 1");

    internal static void UpdateHideWireds(IQueryAdapter dbClient, int roomId, bool hideWireds) => dbClient.RunQuery("UPDATE `room` SET allow_hidewireds = '" + (hideWireds ? 1 : 0) + "' WHERE id = '" + roomId + "'");

    internal static void UpdateOwner(IQueryAdapter dbClient, int roomId, string username)
    {
        dbClient.SetQuery("UPDATE `room` SET owner = @newowner WHERE id = '" + roomId + "'");
        dbClient.AddParameter("newowner", username);
        dbClient.RunQuery();
    }

    internal static void UpdatePrice(IQueryAdapter dbClient, int roomId, int price)
    {
        dbClient.SetQuery("UPDATE `room` SET price = @price WHERE id = @roomid LIMIT 1");
        dbClient.AddParameter("roomid", roomId);
        dbClient.AddParameter("price", price);
        dbClient.RunQuery();
    }

    internal static void UpdateUsersMax(IQueryAdapter dbClient, int roomId, int maxUsers) => dbClient.RunQuery("UPDATE `room` SET users_max = '" + maxUsers + "' WHERE id = '" + roomId + "'");

    internal static DataRow GetOne(IQueryAdapter dbClient, int roomId)
    {
        dbClient.SetQuery("SELECT room.`id`, room.`caption`, room.`owner`, room.`description`, room.`category`, room.`state`, room.`users_max`, room.`model_name`, room.`score`, room.`tags`, room.`password`, room.`wallpaper`, room.`floor`, room.`landscape`, room.`allow_pets`, room.`allow_pets_eat`, room.`allow_walkthrough`, room.`allow_hidewall`, room.`wallthick`, room.`floorthick`, room.`moderation_mute_fuse`, room.`allow_rightsoverride`, room.`moderation_kick_fuse`, room.`moderation_ban_fuse`, room.`group_id`, room.`chat_type`, room.`chat_balloon`, room.`chat_speed`, room.`chat_max_distance`, room.`chat_flood_protection`, room.`troc_status`, room.`users_now`, room.`allow_hidewireds`, room.`price`, room.`wired_security`, user.`id` as `owner_id`, user.`langue` FROM `room` LEFT JOIN user ON room.owner = user.username  WHERE room.id = '" + roomId + "'");
        return dbClient.GetRow();
    }

    internal static int Insert(IQueryAdapter dbClient, string name, string desc, string username, string model, int category, int maxVisitors, int tradeSettings)
    {
        dbClient.SetQuery("INSERT INTO `room` (caption,description,owner,model_name,category,users_max,troc_status) VALUES (@caption, @desc, @username, @model, @cat, @usmax, '" + tradeSettings + "')");
        dbClient.AddParameter("caption", name);
        dbClient.AddParameter("desc", desc);
        dbClient.AddParameter("username", username);
        dbClient.AddParameter("model", model);
        dbClient.AddParameter("cat", category);
        dbClient.AddParameter("usmax", maxVisitors);
        return Convert.ToInt32(dbClient.InsertQuery());
    }

    internal static void UpdateUsersNow(IQueryAdapter dbClient, int roomId, int count) => dbClient.RunQuery("UPDATE `room` SET users_now = '" + count + "' WHERE id = '" + roomId + "'");

    internal static void UpdateState(IQueryAdapter dbClient, int roomId) => dbClient.RunQuery("UPDATE `room` SET state = 'locked' WHERE id = '" + roomId + "'");

    internal static void UpdateCaptionDescTags(IQueryAdapter dbClient, int roomId) => dbClient.RunQuery("UPDATE `room` SET caption = 'Cet appart ne respect par les conditions dutilisation', description = 'Cet appart ne respect par les conditions dutilisation', tags = '' WHERE id = '" + roomId + "'");
}
