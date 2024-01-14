namespace WibboEmulator.Games.Navigators;
using System.Data;
using WibboEmulator.Communication.Packets.Outgoing;
using WibboEmulator.Database.Daos.Room;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal static class NavigatorHandler
{
    public static void Search(ServerPacket message, SearchResultList searchResult, string searchData, GameClient session, int fetchLimit)
    {
        //Switching by categorys.
        switch (searchResult.CategoryType)
        {
            default:
                message.WriteInteger(0);
                break;

            case NavigatorCategoryType.Query:
            {
                if (searchData.ToLower().StartsWith("owner:"))
                {
                    if (searchData.Length > 0)
                    {
                        List<RoomEntity> roomList = null;
                        using (var dbClient = WibboEnvironment.GetDatabaseManager().Connection())
                        {
                            if (searchData.ToLower().StartsWith("owner:"))
                            {
                                roomList = RoomDao.GetAllSearchByUsername(dbClient, searchData.Remove(0, 6));
                            }
                        }

                        var results = new List<RoomData>();
                        if (roomList != null && roomList.Count != 0)
                        {
                            foreach (var room in roomList)
                            {
                                var roomData = WibboEnvironment.GetGame().GetRoomManager().FetchRoomData(room.Id, room);
                                if (roomData != null && !results.Contains(roomData))
                                {
                                    results.Add(roomData);
                                }
                            }
                        }

                        message.WriteInteger(results.Count);
                        foreach (var data in results.ToList())
                        {
                            RoomAppender.WriteRoom(message, data);
                        }
                    }
                }
                else if (searchData.ToLower().StartsWith("tag:"))
                {
                    searchData = searchData.Remove(0, 4);
                    ICollection<RoomData> tagMatches = WibboEnvironment.GetGame().GetRoomManager().SearchTaggedRooms(searchData);

                    message.WriteInteger(tagMatches.Count);
                    foreach (var data in tagMatches.ToList())
                    {
                        RoomAppender.WriteRoom(message, data);
                    }
                }
                else if (searchData.ToLower().StartsWith("group:"))
                {
                    searchData = searchData.Remove(0, 6);
                    ICollection<RoomData> groupRooms = WibboEnvironment.GetGame().GetRoomManager().SearchGroupRooms(searchData);

                    message.WriteInteger(groupRooms.Count);
                    foreach (var data in groupRooms.ToList())
                    {
                        RoomAppender.WriteRoom(message, data);
                    }
                }
                else
                {
                    if (searchData.Length > 0)
                    {
                        List<RoomEntity> roomList = null;
                        using (var dbClient = WibboEnvironment.GetDatabaseManager().Connection())
                        {
                            roomList = RoomDao.GetAllSearch(dbClient, searchData);
                        }

                        var results = new List<RoomData>();
                        if (roomList != null && roomList.Count != 0)
                        {
                            foreach (var room in roomList)
                            {
                                if (room.State == RoomState.Hide)
                                {
                                    continue;
                                }

                                var rData = WibboEnvironment.GetGame().GetRoomManager().FetchRoomData(room.Id, room);
                                if (rData != null && !results.Contains(rData))
                                {
                                    results.Add(rData);
                                }
                            }
                        }

                        message.WriteInteger(results.Count);
                        foreach (var data in results.ToList())
                        {
                            RoomAppender.WriteRoom(message, data);
                        }
                    }
                }

                break;
            }

            case NavigatorCategoryType.Featured:
            case NavigatorCategoryType.FeaturedGame:
            case NavigatorCategoryType.FeaturedNovelty:
            case NavigatorCategoryType.FeaturedHelpSecurity:
            case NavigatorCategoryType.FeaturedRun:
            case NavigatorCategoryType.FeaturedCasino:
                var rooms = new List<RoomData>();
                var featured = WibboEnvironment.GetGame().GetNavigator().GetFeaturedRooms(session.Langue);
                foreach (var featuredItem in featured.ToList())
                {
                    if (featuredItem == null)
                    {
                        continue;
                    }

                    if (featuredItem.CategoryType != searchResult.CategoryType)
                    {
                        continue;
                    }

                    var data = WibboEnvironment.GetGame().GetRoomManager().GenerateRoomData(featuredItem.RoomId);
                    if (data == null)
                    {
                        continue;
                    }

                    if (!rooms.Contains(data))
                    {
                        rooms.Add(data);
                    }
                }

                message.WriteInteger(rooms.Count);
                foreach (var data in rooms.ToList())
                {
                    RoomAppender.WriteRoom(message, data);
                }
                break;

            case NavigatorCategoryType.Popular:
            {
                var popularRooms = new List<RoomData>();

                popularRooms.AddRange(WibboEnvironment.GetGame().GetRoomManager().GetPopularRooms(-1, 20, session.Langue)); //FetchLimit

                message.WriteInteger(popularRooms.Count);
                foreach (var data in popularRooms.ToList())
                {
                    RoomAppender.WriteRoom(message, data);
                }
                break;
            }

            case NavigatorCategoryType.Recommended:
            {
                var recommendedRooms = WibboEnvironment.GetGame().GetRoomManager().GetRecommendedRooms(fetchLimit);

                message.WriteInteger(recommendedRooms.Count);
                foreach (var data in recommendedRooms.ToList())
                {
                    RoomAppender.WriteRoom(message, data);
                }
                break;
            }

            case NavigatorCategoryType.Category:
            {
                var getRoomsByCategory = WibboEnvironment.GetGame().GetRoomManager().GetRoomsByCategory(searchResult.Id, fetchLimit);

                message.WriteInteger(getRoomsByCategory.Count);
                foreach (var data in getRoomsByCategory.ToList())
                {
                    RoomAppender.WriteRoom(message, data);
                }
                break;
            }

            case NavigatorCategoryType.MyRooms:

                var myRooms = new List<RoomData>();

                foreach (var roomId in session.User.UsersRooms)
                {
                    var data = WibboEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
                    if (data == null)
                    {
                        continue;
                    }

                    if (!myRooms.Contains(data))
                    {
                        myRooms.Add(data);
                    }
                }

                message.WriteInteger(myRooms.Count);
                foreach (var data in myRooms.OrderBy(a => a.Name).ToList())
                {
                    RoomAppender.WriteRoom(message, data);
                }
                break;

            case NavigatorCategoryType.MyFavorites:
                var favourites = new List<RoomData>();
                foreach (var roomId in session.User.FavoriteRooms)
                {
                    var data = WibboEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
                    if (data == null)
                    {
                        continue;
                    }

                    if (!favourites.Contains(data))
                    {
                        favourites.Add(data);
                    }
                }

                favourites = favourites.Take(fetchLimit).ToList();

                message.WriteInteger(favourites.Count);
                foreach (var data in favourites.ToList())
                {
                    RoomAppender.WriteRoom(message, data);
                }
                break;

            case NavigatorCategoryType.MyGroups:
                var myGroups = new List<RoomData>();

                foreach (var groupId in session.User.MyGroups.ToList())
                {
                    if (!WibboEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out var group))
                    {
                        continue;
                    }

                    var data = WibboEnvironment.GetGame().GetRoomManager().GenerateRoomData(group.RoomId);
                    if (data == null)
                    {
                        continue;
                    }

                    if (!myGroups.Contains(data))
                    {
                        myGroups.Add(data);
                    }
                }

                myGroups = myGroups.Take(fetchLimit).ToList();

                message.WriteInteger(myGroups.Count);
                foreach (var data in myGroups.ToList())
                {
                    RoomAppender.WriteRoom(message, data);
                }
                break;

            /*case NavigatorCategoryType.MY_FRIENDS_ROOMS:
                List<RoomData> MyFriendsRooms = new List<RoomData>();
                foreach (MessengerBuddy buddy in session.GetUser().GetMessenger().GetFriends().Where(p => p.))
                {
                    if (buddy == null || !buddy.InRoom || buddy.UserId == session.GetUser().Id)
                        continue;

                    if (!MyFriendsRooms.Contains(buddy.CurrentRoom.RoomData))
                        MyFriendsRooms.Add(buddy.CurrentRoom.RoomData);
                }

                Message.WriteInteger(MyFriendsRooms.Count);
                foreach (RoomData Data in MyFriendsRooms.ToList())
                {
                    RoomAppender.WriteRoom(Message, Data);
                }
                break;*/

            case NavigatorCategoryType.MyRights:
                var myRights = new List<RoomData>();

                foreach (var roomId in session.User.RoomRightsList)
                {
                    var data = WibboEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
                    if (data == null)
                    {
                        continue;
                    }

                    if (!myRights.Contains(data))
                    {
                        myRights.Add(data);
                    }
                }

                myRights = myRights.Take(fetchLimit).ToList();

                message.WriteInteger(myRights.Count);
                foreach (var data in myRights.ToList())
                {
                    RoomAppender.WriteRoom(message, data);
                }
                break;
        }
    }
}
