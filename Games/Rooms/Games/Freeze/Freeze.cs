namespace WibboEmulator.Games.Rooms.Games.Freeze;
using System.Collections;
using System.Drawing;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Avatar;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Freeze;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Session;
using WibboEmulator.Games.Items;
using WibboEmulator.Games.Rooms.Games.Teams;
using WibboEmulator.Games.Rooms.Map;

public class Freeze
{
    private Room _room;
    private readonly Dictionary<int, Item> _freezeBlocks;

    public Freeze(Room room)
    {
        this._room = room;
        this._freezeBlocks = [];
        this.IsGameActive = false;
    }

    public bool IsGameActive { get; private set; }

    public void StartGame()
    {
        if (this.IsGameActive)
        {
            return;
        }

        this.IsGameActive = true;
        this.CountTeamPoints();
        this.ResetGame();
    }

    public void StopGame()
    {
        if (!this.IsGameActive)
        {
            return;
        }

        this.IsGameActive = false;

        var winningTeam = this._room.GameManager.WinningTeam;

        foreach (var user in this._room.TeamManager.AllPlayers)
        {
            this.EndGame(user, winningTeam);
        }
    }

    private void EndGame(RoomUser roomUser, TeamType winningTeam)
    {
        if (roomUser.Team == winningTeam && winningTeam != TeamType.None)
        {
            this._room.SendPacket(new ActionComposer(roomUser.VirtualId, 1));
        }
        else if (roomUser.Team != TeamType.None)
        {
            var firstTile = this.GetFirstTile(roomUser.X, roomUser.Y);

            if (firstTile == null)
            {
                return;
            }
            roomUser.ApplyEffect(0);
            var managerForFreeze = this._room.TeamManager;
            managerForFreeze.OnUserLeave(roomUser);

            roomUser.Client.SendPacket(new IsPlayingComposer(false));

            this._room.
            GameManager.UpdateGatesTeamCounts();

            roomUser.Team = TeamType.None;

            if (this._room.GameItemHandler.ExitTeleport != null)
            {
                GameMap.TeleportToItem(roomUser, this._room.GameItemHandler.ExitTeleport);
            }

            roomUser.Freezed = false;
            roomUser.SetStep = false;
            roomUser.IsWalking = false;
            roomUser.UpdateNeeded = true;
            roomUser.FreezeLives = 0;
        }
    }

    public void CycleUser(RoomUser user)
    {
        if (!this.IsGameActive && !user.Freezed && !user.ShieldActive)
        {
            return;
        }

        if (user.Freezed)
        {
            user.FreezeCounter++;
            if (user.FreezeCounter > 10)
            {
                user.Freezed = false;
                user.FreezeCounter = 0;
                ActivateShield(user);
            }
        }
        if (user.ShieldActive)
        {
            user.ShieldCounter++;
            if (user.ShieldCounter <= 10)
            {
                return;
            }

            user.ShieldActive = false;
            user.ShieldCounter = 10;
            user.ApplyEffect((int)user.Team + 39);
        }
    }

    public void ResetGame()
    {
        foreach (Item roomItem in (IEnumerable)this._freezeBlocks.Values)
        {
            if (!string.IsNullOrEmpty(roomItem.ExtraData))
            {
                roomItem.ExtraData = "";
                roomItem.UpdateState(false);
                this._room.GameMap.UpdateMapForItem(roomItem);
            }
        }
    }

    public void OnWalkFreezeBlock(Item roomitem, RoomUser user)
    {
        if (!this.IsGameActive || user.Team == TeamType.None)
        {
            return;
        }

        if (roomitem.FreezePowerUp != FreezePowerUp.None)
        {
            this.PickUpPowerUp(roomitem, user);
        }
    }

    private void CountTeamPoints()
    {
        foreach (var user in this._room.TeamManager.AllPlayers)
        {
            this.CountTeamPoint(user);
        }
    }

    private void CountTeamPoint(RoomUser roomUser)
    {
        if (roomUser == null || roomUser.Client == null)
        {
            return;
        }

        var firstTile = this.GetFirstTile(roomUser.X, roomUser.Y);

        if (firstTile == null)
        {
            this.EndGame(roomUser, TeamType.None);
            return;
        }

        roomUser.BanzaiPowerUp = FreezePowerUp.None;
        roomUser.FreezeLives = 3;
        roomUser.ShieldActive = false;
        roomUser.ShieldCounter = 11;

        this._room.
        GameManager.AddPointToTeam(roomUser.Team, 40, null);

        roomUser.Client.SendPacket(new UpdateFreezeLivesComposer(roomUser.VirtualId, roomUser.FreezeLives));
    }

    public void ThrowBall(Item item, RoomUser roomUserByUserId)
    {
        if (Math.Abs(roomUserByUserId.X - item.X) >= 2 || Math.Abs(roomUserByUserId.Y - item.Y) >= 2)
        {
            return;
        }

        if (roomUserByUserId.Team == TeamType.None || !this.IsGameActive)
        {
            return;
        }

        var tile = this.GetFirstTile(item.X, item.Y);

        if (tile == null)
        {
            return;
        }

        if (tile.InteractionCountHelper == 0)
        {
            tile.InteractionCountHelper = 1;
            tile.ExtraData = "1000";
            tile.UpdateState();
            tile.InteractingUser = roomUserByUserId.UserId;
            tile.FreezePowerUp = roomUserByUserId.BanzaiPowerUp;
            tile.ReqUpdate(5);
            roomUserByUserId.CountFreezeBall -= 1;
            switch (roomUserByUserId.BanzaiPowerUp)
            {
                case FreezePowerUp.GreenArrow:
                case FreezePowerUp.OrangeSnowball:
                case FreezePowerUp.BlueArrow:
                    roomUserByUserId.BanzaiPowerUp = FreezePowerUp.None;
                    break;
            }
        }
    }

    private Item GetFirstTile(int x, int y)
    {
        foreach (var roomItem in this._room.GameMap.GetCoordinatedItems(new Point(x, y)))
        {
            if (roomItem.ItemData.InteractionType == InteractionType.FREEZE_TILE)
            {
                return roomItem;
            }
        }
        return null;
    }

    public void OnFreezeTiles(Item item, FreezePowerUp powerUp, int userID)
    {
        if (this._room.RoomUserManager.GetRoomUserByUserId(userID) == null)
        {
            return;
        }

        List<Item> items;
        switch (powerUp)
        {
            case FreezePowerUp.BlueArrow:
                items = this.GetVerticalItems(item.X, item.Y, 5);
                break;
            case FreezePowerUp.GreenArrow:
                items = this.GetDiagonalItems(item.X, item.Y, 5);
                break;
            case FreezePowerUp.OrangeSnowball:
                items = this.GetVerticalItems(item.X, item.Y, 5);
                items.AddRange(this.GetDiagonalItems(item.X, item.Y, 5));
                break;
            default:
                items = this.GetVerticalItems(item.X, item.Y, 3);
                break;
        }
        this.HandleBanzaiFreezeItems(items);
    }

    private static void ActivateShield(RoomUser user)
    {
        user.ApplyEffect((int)user.Team + 48);
        user.ShieldActive = true;
        user.ShieldCounter = 0;
    }

    private void HandleBanzaiFreezeItems(List<Item> items)
    {
        foreach (var roomItem in items)
        {
            switch (roomItem.ItemData.InteractionType)
            {
                case InteractionType.FREEZE_TILE_BLOCK:
                    this.SetRandomPowerUp(roomItem);
                    roomItem.UpdateState(false);
                    continue;
                case InteractionType.FREEZE_TILE:
                    roomItem.ExtraData = "11000";
                    roomItem.UpdateState(false);
                    continue;
                default:
                    continue;
            }
        }
    }

    private void SetRandomPowerUp(Item item)
    {
        if (!string.IsNullOrEmpty(item.ExtraData))
        {
            return;
        }

        switch (WibboEnvironment.GetRandomNumber(1, 14))
        {
            case 2:
                item.ExtraData = "2000";
                item.FreezePowerUp = FreezePowerUp.BlueArrow;
                break;
            case 3:
                item.ExtraData = "3000";
                item.FreezePowerUp = FreezePowerUp.Snowballs;
                break;
            case 4:
                item.ExtraData = "4000";
                item.FreezePowerUp = FreezePowerUp.GreenArrow;
                break;
            case 5:
                item.ExtraData = "5000";
                item.FreezePowerUp = FreezePowerUp.OrangeSnowball;
                break;
            case 6:
                item.ExtraData = "6000";
                item.FreezePowerUp = FreezePowerUp.Heart;
                break;
            case 7:
                item.ExtraData = "7000";
                item.FreezePowerUp = FreezePowerUp.Shield;
                break;
            default:
                item.ExtraData = "1000";
                item.FreezePowerUp = FreezePowerUp.None;
                break;
        }
        this._room.GameMap.UpdateMapForItem(item);
        item.UpdateState(false);
    }

    private void PickUpPowerUp(Item item, RoomUser user)
    {
        switch (item.FreezePowerUp)
        {
            case FreezePowerUp.BlueArrow:
            case FreezePowerUp.GreenArrow:
            case FreezePowerUp.OrangeSnowball:
                user.BanzaiPowerUp = item.FreezePowerUp;
                break;
            case FreezePowerUp.Shield:
                ActivateShield(user);
                break;
            case FreezePowerUp.Heart:
                if (user.FreezeLives < 3)
                {
                    user.FreezeLives++;
                    this._room.GameManager.AddPointToTeam(user.Team, 10, user);
                }

                user.Client.SendPacket(new UpdateFreezeLivesComposer(user.VirtualId, user.FreezeLives));
                break;
            case FreezePowerUp.Snowballs:
                user.CountFreezeBall += 1;
                break;
        }
        item.FreezePowerUp = FreezePowerUp.None;
        item.ExtraData = "1" + item.ExtraData;
        item.UpdateState(false);
    }

    public void AddFreezeBlock(Item item)
    {
        if (this._freezeBlocks.ContainsKey(item.Id))
        {
            _ = this._freezeBlocks.Remove(item.Id);
        }

        this._freezeBlocks.Add(item.Id, item);
    }

    public void RemoveFreezeBlock(int itemID) => this._freezeBlocks.Remove(itemID);

    private void HandleUserFreeze(Point point)
    {
        var user = this._room.RoomUserManager.GetUserForSquare(point.X, point.Y);
        if (user == null || (user.IsWalking && user.SetX != point.X && user.SetY != point.Y))
        {
            return;
        }

        this.FreezeUser(user);
    }

    private void FreezeUser(RoomUser user)
    {
        if (user.IsBot || user.ShieldActive || user.Team == TeamType.None || !this.IsGameActive)
        {
            return;
        }

        if (user.Freezed)
        {
            user.Freezed = false;
            user.ApplyEffect((int)user.Team + 39);
        }
        else
        {
            user.Freezed = true;
            user.FreezeCounter = 0;
            --user.FreezeLives;
            if (user.FreezeLives <= 0)
            {
                user.Client.SendPacket(new UpdateFreezeLivesComposer(user.VirtualId, user.FreezeLives));

                user.ApplyEffect(0);

                this._room.
                GameManager.AddPointToTeam(user.Team, -20, user);

                var managerForFreeze = this._room.TeamManager;
                managerForFreeze.OnUserLeave(user);

                user.Client.SendPacket(new IsPlayingComposer(false));

                this._room.
                GameManager.UpdateGatesTeamCounts();
                user.Team = TeamType.None;

                if (this._room.GameItemHandler.ExitTeleport != null)
                {
                    GameMap.TeleportToItem(user, this._room.GameItemHandler.ExitTeleport);
                }

                user.Freezed = false;
                user.SetStep = false;
                user.IsWalking = false;
                user.UpdateNeeded = true;

                if (managerForFreeze.BlueTeam.Count <= 0 && managerForFreeze.RedTeam.Count <= 0 && managerForFreeze.GreenTeam.Count <= 0 && managerForFreeze.YellowTeam.Count > 0)
                {
                    this.StopGame();
                }
                else if (managerForFreeze.BlueTeam.Count > 0 && managerForFreeze.RedTeam.Count <= 0 && managerForFreeze.GreenTeam.Count <= 0 && managerForFreeze.YellowTeam.Count <= 0)
                {
                    this.StopGame();
                }
                else if (managerForFreeze.BlueTeam.Count <= 0 && managerForFreeze.RedTeam.Count > 0 && managerForFreeze.GreenTeam.Count <= 0 && managerForFreeze.YellowTeam.Count <= 0)
                {
                    this.StopGame();
                }
                else
                {
                    if (managerForFreeze.BlueTeam.Count > 0 || managerForFreeze.RedTeam.Count > 0 || managerForFreeze.GreenTeam.Count <= 0 || managerForFreeze.YellowTeam.Count > 0)
                    {
                        return;
                    }

                    this.StopGame();
                }
            }
            else
            {
                this._room.GameManager.AddPointToTeam(user.Team, -10, user);
                user.ApplyEffect(12);

                user.Client.SendPacket(new UpdateFreezeLivesComposer(user.VirtualId, user.FreezeLives));
            }
        }
    }

    private List<Item> GetVerticalItems(int x, int y, int length)
    {
        var list = new List<Item>();
        for (var index = 0; index < length; ++index)
        {
            var point = new Point(x + index, y);
            var itemsForSquare = this.GetItemsForSquare(point);
            if (SquareGotFreezeTile(itemsForSquare))
            {
                this.HandleUserFreeze(point);
                list.AddRange(itemsForSquare);
                if (SquareGotFreezeBlock(itemsForSquare))
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        for (var index = 1; index < length; ++index)
        {
            var point = new Point(x, y + index);
            var itemsForSquare = this.GetItemsForSquare(point);
            if (SquareGotFreezeTile(itemsForSquare))
            {
                this.HandleUserFreeze(point);
                list.AddRange(itemsForSquare);
                if (SquareGotFreezeBlock(itemsForSquare))
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        for (var index = 1; index < length; ++index)
        {
            var point = new Point(x - index, y);
            var itemsForSquare = this.GetItemsForSquare(point);
            if (SquareGotFreezeTile(itemsForSquare))
            {
                this.HandleUserFreeze(point);
                list.AddRange(itemsForSquare);
                if (SquareGotFreezeBlock(itemsForSquare))
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        for (var index = 1; index < length; ++index)
        {
            var point = new Point(x, y - index);
            var itemsForSquare = this.GetItemsForSquare(point);
            if (SquareGotFreezeTile(itemsForSquare))
            {
                this.HandleUserFreeze(point);
                list.AddRange(itemsForSquare);
                if (SquareGotFreezeBlock(itemsForSquare))
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        return list;
    }

    private List<Item> GetDiagonalItems(int x, int y, int length)
    {
        var list = new List<Item>();
        for (var index = 0; index < length; ++index)
        {
            var point = new Point(x + index, y + index);
            var itemsForSquare = this.GetItemsForSquare(point);
            if (SquareGotFreezeTile(itemsForSquare))
            {
                this.HandleUserFreeze(point);
                list.AddRange(itemsForSquare);
                if (SquareGotFreezeBlock(itemsForSquare))
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        for (var index = 0; index < length; ++index)
        {
            var point = new Point(x - index, y - index);
            var itemsForSquare = this.GetItemsForSquare(point);
            if (SquareGotFreezeTile(itemsForSquare))
            {
                this.HandleUserFreeze(point);
                list.AddRange(itemsForSquare);
                if (SquareGotFreezeBlock(itemsForSquare))
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        for (var index = 0; index < length; ++index)
        {
            var point = new Point(x - index, y + index);
            var itemsForSquare = this.GetItemsForSquare(point);
            if (SquareGotFreezeTile(itemsForSquare))
            {
                this.HandleUserFreeze(point);
                list.AddRange(itemsForSquare);
                if (SquareGotFreezeBlock(itemsForSquare))
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        for (var index = 0; index < length; ++index)
        {
            var point = new Point(x + index, y - index);
            var itemsForSquare = this.GetItemsForSquare(point);
            if (SquareGotFreezeTile(itemsForSquare))
            {
                this.HandleUserFreeze(point);
                list.AddRange(itemsForSquare);
                if (SquareGotFreezeBlock(itemsForSquare))
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        return list;
    }

    private List<Item> GetItemsForSquare(Point point) => this._room.GameMap.GetCoordinatedItems(point);

    private static bool SquareGotFreezeTile(List<Item> items)
    {
        foreach (var roomItem in items)
        {
            if (roomItem.ItemData.InteractionType == InteractionType.FREEZE_TILE)
            {
                return true;
            }
        }
        return false;
    }

    private static bool SquareGotFreezeBlock(List<Item> items)
    {
        foreach (var roomItem in items)
        {
            if (roomItem.ItemData.InteractionType == InteractionType.FREEZE_TILE_BLOCK)
            {
                return true;
            }
        }
        return false;
    }

    public void Destroy()
    {
        this._freezeBlocks.Clear();
        this._room = null;
    }
}
