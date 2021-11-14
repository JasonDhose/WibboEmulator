using Butterfly.Database.Interfaces;
using System.Data;

namespace Butterfly.Database.Daos
{
    class EmulatorHotelviewPromoDao
    {
        internal static DataTable GetAll(IQueryAdapter dbClient)
        {
            dbClient.SetQuery("SELECT `index`, `header`, `body`, `button`, `in_game_promo`, `special_action`, `image`, `enabled` from `emulator_hotelview_promo` WHERE `enabled` = '1' ORDER BY `index` ASC");
            return dbClient.GetTable();
        }
    }
}