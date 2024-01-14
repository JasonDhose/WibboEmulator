namespace WibboEmulator.Communication.Packets.Incoming.Rooms.AI.Pets;
using System.Drawing;
using WibboEmulator.Communication.Packets.Outgoing.Inventory.Pets;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Engine;
using WibboEmulator.Database.Daos.Bot;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms.AI;

internal sealed class PickUpPetEvent : IPacketEvent
{
    public double Delay => 250;

    public void Parse(GameClient session, ClientPacket packet)
    {
        if (!session.User.InRoom)
        {
            return;
        }

        if (session == null || session.User == null || session.User.InventoryComponent == null)
        {
            return;
        }


        if (!WibboEnvironment.GetGame().GetRoomManager().TryGetRoom(session.User.CurrentRoomId, out var room))
        {
            return;
        }

        var petId = packet.PopInt();

        if (!room.RoomUserManager.TryGetPet(petId, out var pet))
        {
            if ((!room.CheckRights(session) && room.RoomData.WhoCanKick != 2 && room.RoomData.Group == null) || (room.RoomData.Group != null && !room.CheckRights(session)))
            {
                return;
            }

            var targetUser = session.User.CurrentRoom.RoomUserManager.GetRoomUserByUserId(petId);
            if (targetUser == null)
            {
                return;
            }

            if (targetUser.Client == null || targetUser.Client.User == null)
            {
                return;
            }

            targetUser.IsTransf = false;

            room.SendPacket(new UserRemoveComposer(targetUser.VirtualId));

            room.SendPacket(new UsersComposer(targetUser));
            return;
        }

        if (session.User.Id != pet.PetData.OwnerId && !room.CheckRights(session, true))
        {
            return;
        }

        if (pet.RidingHorse)
        {
            var userRiding = room.RoomUserManager.GetRoomUserByVirtualId(pet.HorseID);
            if (userRiding != null)
            {
                userRiding.RidingHorse = false;
                userRiding.ApplyEffect(-1);
                userRiding.MoveTo(new Point(userRiding.X + 1, userRiding.Y + 1));
            }
            else
            {
                pet.RidingHorse = false;
            }
        }

        var petData = pet.PetData;

        pet.RoomId = 0;
        petData.PlacedInRoom = false;

        using (var dbClient = WibboEnvironment.GetDatabaseManager().Connection())
        {
            BotPetDao.UpdateRoomId(dbClient, petData.PetId, 0);
        }

        if (petData.OwnerId != session.User.Id)
        {
            var target = WibboEnvironment.GetGame().GetGameClientManager().GetClientByUserID(petData.OwnerId);
            if (target != null)
            {
                _ = target.User.InventoryComponent.TryAddPet(pet.PetData);
                room.RoomUserManager.RemoveBot(pet.VirtualId, false);

                target.SendPacket(new PetInventoryComposer(target.User.InventoryComponent.GetPets()));
                return;
            }
        }
        else
        {
            _ = session.User.InventoryComponent.TryAddPet(pet.PetData);
            room.RoomUserManager.RemoveBot(pet.VirtualId, false);
            session.SendPacket(new PetInventoryComposer(session.User.InventoryComponent.GetPets()));
        }
    }
}
