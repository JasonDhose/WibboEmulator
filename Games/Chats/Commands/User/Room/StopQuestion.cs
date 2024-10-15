namespace WibboEmulator.Games.Chats.Commands.User.Room;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Polls;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal sealed class StopQuestion : IChatCommand
{
    public void Execute(GameClient session, Room room, RoomUser userRoom, string[] parameters)
    {
        room.SendPacket(new QuestionFinishedComposer(room.VotedNoCount, room.VotedYesCount));

        session.SendWhisper("Question terminée!");
    }
}
