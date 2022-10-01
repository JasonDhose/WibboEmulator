using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;
using WibboEmulator.Games.Rooms.Games;

namespace WibboEmulator.Communication.Packets.Incoming.Structure
{
    internal class AvatarEffectSelectedEvent : IPacketEvent
    {
        public double Delay => 500;

        public void Parse(GameClient Session, ClientPacket Packet)
        {
            int NumEnable = Packet.PopInt();

            if (NumEnable < 0)
            {
                return;
            }

            if (!WibboEnvironment.GetGame().GetEffectManager().HaveEffect(NumEnable, Session.GetUser().HasPermission("perm_god")))
            {
                return;
            }

            Room Room = Session.GetUser().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByUserId(Session.GetUser().Id);

            if (User == null)
            {
                return;
            }

            int CurrentEnable = User.CurrentEffect;
            if (CurrentEnable == 28 || CurrentEnable == 29 || CurrentEnable == 30 || CurrentEnable == 37 || CurrentEnable == 184 || CurrentEnable == 77 || CurrentEnable == 103
                || CurrentEnable == 40 || CurrentEnable == 41 || CurrentEnable == 42 || CurrentEnable == 43
                || CurrentEnable == 49 || CurrentEnable == 50 || CurrentEnable == 51 || CurrentEnable == 52
                || CurrentEnable == 33 || CurrentEnable == 34 || CurrentEnable == 35 || CurrentEnable == 36)
            {
                return;
            }

            if (User.Team != TeamType.NONE || User.InGame)
            {
                return;
            }

            User.ApplyEffect(NumEnable);
        }
    }
}