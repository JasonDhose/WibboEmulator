namespace WibboEmulator.Communication.Packets.Incoming.Marketplace;
using System.Data;
using WibboEmulator.Communication.Packets.Outgoing.Catalog;
using WibboEmulator.Communication.Packets.Outgoing.Inventory.Purse;
using WibboEmulator.Communication.Packets.Outgoing.MarketPlace;
using WibboEmulator.Core.Language;
using WibboEmulator.Database;
using WibboEmulator.Database.Daos.Catalog;
using WibboEmulator.Database.Daos.User;
using WibboEmulator.Games.Catalogs.Marketplace;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Items;

internal sealed class BuyOfferEvent : IPacketEvent
{
    public double Delay => 1000;

    public void Parse(GameClient Session, ClientPacket packet)
    {
        var offerId = packet.PopInt();

        using var dbClient = DatabaseManager.Connection;

        var offer = CatalogMarketplaceOfferDao.GetOneByOfferId(dbClient, offerId);

        if (offer == null)
        {
            ReloadOffers(Session, dbClient);
            return;
        }

        if (offer.State == 2)
        {
            Session.SendNotification(LanguageManager.TryGetValue("notif.buyoffer.error.1", Session.Language));
            ReloadOffers(Session, dbClient);
            return;
        }

        if (MarketplaceManager.FormatTimestamp() > offer.Timestamp)
        {
            Session.SendNotification(LanguageManager.TryGetValue("notif.buyoffer.error.2", Session.Language));
            ReloadOffers(Session, dbClient);
            return;
        }

        if (!ItemManager.GetItem(offer.ItemId, out var item))
        {
            Session.SendNotification(LanguageManager.TryGetValue("notif.buyoffer.error.3", Session.Language));
            ReloadOffers(Session, dbClient);
            return;
        }
        else
        {
            if (offer.UserId == Session.User.Id)
            {
                Session.SendNotification(LanguageManager.TryGetValue("notif.buyoffer.error.4", Session.Language));
                return;
            }

            if (offer.TotalPrice > Session.User.WibboPoints)
            {
                Session.SendNotification(LanguageManager.TryGetValue("notif.buyoffer.error.5", Session.Language));
                return;
            }

            Session.User.WibboPoints -= offer.TotalPrice;
            Session.SendPacket(new ActivityPointNotificationComposer(Session.User.WibboPoints, 0, 105));

            UserDao.UpdateRemovePoints(dbClient, Session.User.Id, offer.TotalPrice);

            var giveItem = ItemFactory.CreateSingleItem(dbClient, item, Session.User, offer.ExtraData, offer.FurniId, offer.LimitedNumber, offer.LimitedStack);
            if (giveItem != null)
            {
                Session.User.InventoryComponent.TryAddItem(giveItem);

                Session.SendPacket(new PurchaseOKComposer());
            }

            CatalogMarketplaceOfferDao.UpdateState(dbClient, offerId);
            CatalogMarketplaceDataDao.Replace(dbClient, item.SpriteId, offer.TotalPrice);

            if (MarketplaceManager.MarketAverages.TryGetValue(item.SpriteId, out var value) && MarketplaceManager.MarketCounts.ContainsKey(item.SpriteId))
            {
                var num3 = value;
                var num4 = MarketplaceManager.MarketAverages[item.SpriteId] += offer.TotalPrice;

                _ = MarketplaceManager.MarketAverages.Remove(item.SpriteId);
                MarketplaceManager.MarketAverages.Add(item.SpriteId, num4);
                _ = MarketplaceManager.MarketCounts.Remove(item.SpriteId);
                MarketplaceManager.MarketCounts.Add(item.SpriteId, num3 + 1);
            }
            else
            {
                if (!MarketplaceManager.MarketAverages.ContainsKey(item.SpriteId))
                {
                    MarketplaceManager.MarketAverages.Add(item.SpriteId, offer.TotalPrice);
                }

                _ = MarketplaceManager.MarketCounts.TryAdd(item.SpriteId, 1);
            }
        }

        ReloadOffers(Session, dbClient);
    }

    private static void ReloadOffers(GameClient Session, IDbConnection dbClient)
    {
        var minCost = -1;
        var maxCost = -1;
        var searchQuery = "";
        var filterMode = 1;

        var offerList = CatalogMarketplaceOfferDao.GetAll(dbClient, searchQuery, minCost, maxCost, filterMode);

        MarketplaceManager.MarketItems.Clear();
        MarketplaceManager.MarketItemKeys.Clear();
        if (offerList.Count != 0)
        {
            foreach (var offer in offerList)
            {
                if (!MarketplaceManager.MarketItemKeys.Contains(offer.OfferId))
                {
                    var item = new MarketOffer(offer.OfferId, offer.SpriteId, offer.TotalPrice, offer.ItemType, offer.LimitedNumber, offer.LimitedStack);
                    MarketplaceManager.MarketItemKeys.Add(offer.OfferId);
                    MarketplaceManager.MarketItems.Add(item);
                }
            }
        }

        var dictionary = new Dictionary<int, MarketOffer>();
        var dictionary2 = new Dictionary<int, int>();

        foreach (var item in MarketplaceManager.MarketItems)
        {
            if (dictionary.TryGetValue(item.SpriteId, out var _))
            {
                if (dictionary[item.SpriteId].TotalPrice > item.TotalPrice)
                {
                    _ = dictionary.Remove(item.SpriteId);
                    dictionary.Add(item.SpriteId, item);
                }

                var num = dictionary2[item.SpriteId];
                _ = dictionary2.Remove(item.SpriteId);
                dictionary2.Add(item.SpriteId, num + 1);
            }
            else
            {
                dictionary.Add(item.SpriteId, item);
                dictionary2.Add(item.SpriteId, 1);
            }
        }

        Session.SendPacket(new MarketPlaceOffersComposer(dictionary, dictionary2));
    }
}
