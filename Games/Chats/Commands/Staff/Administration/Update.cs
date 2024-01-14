namespace WibboEmulator.Games.Chats.Commands.Staff.Administration;
using WibboEmulator.Communication.Packets.Outgoing.Catalog;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Chat;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal sealed class Update : IChatCommand
{
    public void Execute(GameClient session, Room room, RoomUser userRoom, string[] parameters)
    {
        if (parameters.Length != 2)
        {
            return;
        }

        var cmd = parameters[1];

        if (string.IsNullOrEmpty(cmd))
        {
            return;
        }

        using var dbClient = WibboEnvironment.GetDatabaseManager().Connection();
        switch (cmd.ToLower())
        {
            case "emuban":
            {
                WibboEnvironment.GetWebSocketManager().ResetBan();
                session.SendWhisper("Réinitialisation des bannissements de l'émulateur");
                break;
            }
            case "landingview":
            case "view":
            case "lv":
            case "vue":
            {
                WibboEnvironment.GetGame().GetHotelView().Init(dbClient);
                session.SendWhisper("Vue et promotion mises à jour");
                break;
            }
            case "hof":
            {
                WibboEnvironment.GetGame().GetHallOFFame().Init(dbClient);
                session.SendWhisper("Hof mises à jour");
                break;
            }
            case "text":
            case "texte":
            case "locale":
            {
                WibboEnvironment.GetLanguageManager().Init(dbClient);
                session.SendWhisper("Local mis à jour");
                break;
            }
            case "autogame":
            {
                WibboEnvironment.GetGame().GetAnimationManager().Init(dbClient);
                session.SendWhisper("Jeux automatique mis à jour");
                break;
            }
            case "lootbox":
            {
                WibboEnvironment.GetGame().GetLootManager().Init(dbClient);
                session.SendWhisper("Lootbox mis à jour");
                break;
            }
            case "rpitems":
            {
                WibboEnvironment.GetGame().GetRoleplayManager().ItemManager.Init(dbClient);
                session.SendWhisper("RP Items mis à jour");
                break;
            }
            case "rpweapon":
            {
                WibboEnvironment.GetGame().GetRoleplayManager().WeaponManager.Init(dbClient);
                session.SendWhisper("RP Weapon mis à jour");
                break;
            }
            case "rpenemy":
            {
                WibboEnvironment.GetGame().GetRoleplayManager().EnemyManager.Init(dbClient);
                session.SendWhisper("RP Enemy mis à jour");
                break;
            }
            case "cmd":
            case "commands":
            {
                WibboEnvironment.GetGame().GetChatManager().GetCommands().Init(dbClient);
                session.SendWhisper("Commands mis à jour");
                break;
            }
            case "chat":
            {
                WibboEnvironment.GetGame().GetChatManager().GetChatStyles().Init(dbClient);
                session.SendWhisper("Chat mis à jour");
                break;
            }
            case "permission":
            {
                WibboEnvironment.GetGame().GetPermissionManager().Init(dbClient);
                session.SendWhisper("Permissions mises à jour !");
                break;
            }
            case "effet":
            case "enable":
            {
                WibboEnvironment.GetGame().GetEffectManager().Init(dbClient);
                session.SendWhisper("Effet mis à jour");
                break;
            }
            case "rp":
            case "roleplay":
            {
                WibboEnvironment.GetGame().GetRoleplayManager().Init(dbClient);
                session.SendWhisper("Role play mis à jour");
                break;
            }
            case "moderation":
            {
                WibboEnvironment.GetGame().GetModerationManager().Init(dbClient);
                session.SendWhisper("Moderation mis à jour");
                WibboEnvironment.GetGame().GetGameClientManager().SendMessageStaff(new WhisperComposer(userRoom.VirtualId, "Les outils de modération viennent d'être mis à jour, reconnectez-vous!", 23));
                break;
            }
            case "catalogue":
            case "cata":
            {
                WibboEnvironment.GetGame().GetItemManager().Init(dbClient);
                WibboEnvironment.GetGame().GetCatalog().Init(dbClient, WibboEnvironment.GetGame().GetItemManager());
                WibboEnvironment.GetGame().GetGameClientManager().SendMessage(new CatalogUpdatedComposer());
                session.SendWhisper("Catalogue mis à jour");
                break;
            }
            case "navigateur":
            case "navi":
            case "navigator":
            {
                WibboEnvironment.GetGame().GetNavigator().Init(dbClient);
                session.SendWhisper("Navigateur mis à jour");
                break;
            }
            case "filter":
            case "filtre":
            {
                WibboEnvironment.GetGame().GetChatManager().GetFilter().Init(dbClient);
                session.SendWhisper("Filtre mis à jour");
                break;
            }
            case "items":
            case "furni":
            {
                WibboEnvironment.GetGame().GetItemManager().Init(dbClient);
                session.SendWhisper("Items mis à jour");
                break;
            }
            case "model":
            {
                WibboEnvironment.GetGame().GetRoomManager().Init(dbClient);
                session.SendWhisper("Model mis à jour");
                break;
            }
            case "mutant":
            case "figure":
            {
                WibboEnvironment.GetFigureManager().Init();
                session.SendWhisper("Mutant/Figure mises à jour");
                break;
            }
            case "setting":
            case "settings":
            {
                WibboEnvironment.GetSettings().Init(dbClient);
                session.SendWhisper("Paramètre mises à jour");
                break;
            }
            case "banner":
            {
                WibboEnvironment.GetGame().GetBannerManager().Init(dbClient);
                session.SendWhisper("Bannière mises à jour");
                break;
            }
            default:
            {
                session.SendWhisper(WibboEnvironment.GetLanguageManager().TryGetValue("cmd.notfound", session.Langue));
                return;
            }
        }
    }
}
