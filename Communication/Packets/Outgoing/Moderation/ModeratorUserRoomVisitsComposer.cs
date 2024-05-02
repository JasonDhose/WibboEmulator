namespace WibboEmulator.Communication.Packets.Outgoing.Moderation;

using WibboEmulator.Games.Rooms;
using WibboEmulator.Games.Users;
using WibboEmulator.Utilities;

internal sealed class ModeratorUserRoomVisitsComposer : ServerPacket
{
    public ModeratorUserRoomVisitsComposer(User user, Dictionary<double, int> visits)
        : base(ServerPacketHeader.MODTOOL_VISITED_ROOMS_USER)
    {
        this.WriteInteger(user.Id);
        this.WriteString(user.Username);
        this.WriteInteger(visits.Count);

        foreach (var visit in visits)
        {
            var roomData = RoomManager.GenerateNullableRoomData(visit.Value);

            this.WriteInteger(roomData.Id);
            this.WriteString(roomData.Name);
            this.WriteInteger(UnixTimestamp.FromUnixTimestamp(visit.Key).Hour);
            this.WriteInteger(UnixTimestamp.FromUnixTimestamp(visit.Key).Minute);
        }
    }
}
