namespace WibboEmulator.Communication.Packets.Incoming.Camera;
using WibboEmulator.Communication.Packets.Outgoing.Camera;
using WibboEmulator.Core;
using WibboEmulator.Games.GameClients;

internal sealed class RenderRoomThumbnailEvent : IPacketEvent
{
    public double Delay => 5000;

    public void Parse(GameClient Session, ClientPacket packet)
    {
        var photoLength = packet.PopInt();

        if (photoLength > 40_000)
        {
            return;
        }

        var photoBinary = packet.ReadBytes(photoLength);

        if (Session.User == null)
        {
            return;
        }

        var room = Session.User.Room;
        if (room == null)
        {
            return;
        }

        var pictureName = $"thumbnail_{room.Id}";

        var photoId = UploadApi.CameraThubmail(photoBinary, pictureName);

        if (string.IsNullOrEmpty(photoId) || pictureName != photoId)
        {
            return;
        }

        Session.SendPacket(new ThumbnailStatusComposer(true, true));
    }
}
