namespace WibboEmulator.Games.Chats.Commands;
using System.Data;
using System.Text;
using WibboEmulator.Core.Language;
using WibboEmulator.Database.Daos.Emulator;
using WibboEmulator.Games.Chats.Commands.Staff.Administration;
using WibboEmulator.Games.Chats.Commands.Staff.Animation;
using WibboEmulator.Games.Chats.Commands.Staff.Gestion;
using WibboEmulator.Games.Chats.Commands.Staff.Moderation;
using WibboEmulator.Games.Chats.Commands.User.Build;
using WibboEmulator.Games.Chats.Commands.User.Casino;
using WibboEmulator.Games.Chats.Commands.User.Info;
using WibboEmulator.Games.Chats.Commands.User.Inventory;
using WibboEmulator.Games.Chats.Commands.User.Premium;
using WibboEmulator.Games.Chats.Commands.User.Premium.Fun;
using WibboEmulator.Games.Chats.Commands.User.Room;
using WibboEmulator.Games.Chats.Commands.User.RP;
using WibboEmulator.Games.Chats.Commands.User.Several;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Moderations;
using WibboEmulator.Games.Permissions;
using WibboEmulator.Games.Rooms;

public class CommandManager
{
    private readonly Dictionary<string, AuthorizationCommands> _commandRegisterInvokeable;
    private readonly Dictionary<string, string> _listCommande;
    private readonly Dictionary<int, IChatCommand> _commands;
    private readonly string _prefix = ":";

    public CommandManager()
    {
        this._commands = new Dictionary<int, IChatCommand>();
        this._commandRegisterInvokeable = new Dictionary<string, AuthorizationCommands>();
        this._listCommande = new Dictionary<string, string>();
    }

    public void Initialize(IDbConnection dbClient)
    {

        this.InitInvokeableRegister(dbClient);

        this.RegisterCommand();
    }

    public bool Parse(GameClient session, RoomUser user, Room room, string message)
    {
        if (!message.StartsWith(this._prefix))
        {
            return false;
        }

        if (message == this._prefix + "commands")
        {
            session.SendHugeNotif(WibboEnvironment.GetGame().GetChatManager().GetCommands().GetCommandList(session, room));
            return true;
        }

        message = message[1..];
        var split = message.Split(' ');

        if (split.Length == 0)
        {
            return false;
        }

        if (!this._commandRegisterInvokeable.TryGetValue(split[0].ToLower(), out var cmdInfo))
        {
            return false;
        }

        if (!this._commands.TryGetValue(cmdInfo.CommandID, out var cmd))
        {
            return false;
        }

        var autorisationType = cmdInfo.UserGotAuthorizationType(session, room);
        switch (autorisationType)
        {
            case 2:
                user.SendWhisperChat(WibboEnvironment.GetLanguageManager().TryGetValue("cmd.authorized.premium", session.Langue));
                return true;
            case 3:
                user.SendWhisperChat(WibboEnvironment.GetLanguageManager().TryGetValue("cmd.authorized.accred", session.Langue));
                return true;
            case 4:
                user.SendWhisperChat(WibboEnvironment.GetLanguageManager().TryGetValue("cmd.authorized.owner", session.Langue));
                return true;
            case 5:
                user.SendWhisperChat(WibboEnvironment.GetLanguageManager().TryGetValue("cmd.authorized.langue", session.Langue));
                return true;
        }
        if (!cmdInfo.UserGotAuthorization(session, room))
        {
            return false;
        }

        if (cmdInfo.UserGotAuthorizationStaffLog())
        {
            ModerationManager.LogStaffEntry(session.User.Id, session.User.Username, room.Id, string.Empty, split[0].ToLower(), string.Format("Tchat commande: {0}", string.Join(" ", split)));
        }

        cmd.Execute(session, room, user, split);
        return true;
    }

    private void InitInvokeableRegister(IDbConnection dbClient)
    {
        this._commandRegisterInvokeable.Clear();

        var emulatorCommandList = EmulatorCommandDao.GetAll(dbClient);

        if (emulatorCommandList.Count == 0)
        {
            return;
        }

        foreach (var emulatorCommand in emulatorCommandList)
        {
            var key = emulatorCommand.Id;
            var rank = emulatorCommand.MinRank;
            var descriptionFr = emulatorCommand.DescriptionFr;
            var descriptionEn = emulatorCommand.DescriptionEn;
            var descriptionBr = emulatorCommand.DescriptionBr;
            var input = emulatorCommand.Input;
            var strArray = input.ToLower().Split(',');

            foreach (var command in strArray)
            {
                if (this._commandRegisterInvokeable.ContainsKey(command))
                {
                    continue;
                }

                this._commandRegisterInvokeable.Add(command, new AuthorizationCommands(key, strArray[0], rank, descriptionFr, descriptionEn, descriptionBr));
            }
        }
    }

    public string GetCommandList(GameClient client, Room room)
    {
        var rank = client.User.Rank + client.User.Langue.ToString();
        if (this._listCommande.TryGetValue(rank, out var value))
        {
            return value;
        }

        var notDoublons = new List<string>();
        var stringBuilder = new StringBuilder();

        foreach (var chatCommand in this._commandRegisterInvokeable.Values)
        {
            if (chatCommand.UserGotAuthorization(client, room) && !notDoublons.Contains(chatCommand.Input))
            {
                if (client.Langue == Language.English)
                {
                    _ = stringBuilder.Append(":" + chatCommand.Input + " - " + chatCommand.DescriptionEn + "\r\r");
                }
                else if (client.Langue == Language.Portuguese)
                {
                    _ = stringBuilder.Append(":" + chatCommand.Input + " - " + chatCommand.DescriptionBr + "\r\r");
                }
                else
                {
                    _ = stringBuilder.Append(":" + chatCommand.Input + " - " + chatCommand.DescriptionFr + "\r\r");
                }

                notDoublons.Add(chatCommand.Input);
            }
        }
        notDoublons.Clear();

        this._listCommande.Add(rank, stringBuilder.ToString());
        return stringBuilder.ToString();
    }

    public void Register(int commandId, IChatCommand command) => this._commands.Add(commandId, command);


    public static string MergeParams(string[] parameters, int start)
    {
        var merged = new StringBuilder();
        for (var i = start; i < parameters.Length; i++)
        {
            if (i > start)
            {
                _ = merged.Append(' ');
            }

            _ = merged.Append(parameters[i]);
        }

        return merged.ToString();
    }

    public void RegisterCommand()
    {
        this._commands.Clear();

        this.Register(1, new Pickall());
        this.Register(2, new Unload());
        this.Register(3, new KickAll());
        this.Register(4, new RoomFreeze());
        this.Register(5, new MaxFloor());
        this.Register(6, new AutoFloor());
        this.Register(157, new WiredLimit());
        this.Register(7, new SetSpeed());
        this.Register(8, new DisableDiagonal());
        this.Register(9, new SetMax());
        this.Register(10, new Override());
        this.Register(11, new Teleport());
        this.Register(12, new Freeze());
        this.Register(13, new RoomMute());
        this.Register(14, new Warp());
        this.Register(15, new RoomMutePet());
        this.Register(16, new SuperPull());
        this.Register(17, new ForceTransfStop());
        this.Register(18, new MakeSayBot());
        this.Register(19, new SetZ());
        this.Register(20, new SetZStop());
        this.Register(21, new DisableOblique());
        this.Register(23, new HideWireds());
        this.Register(24, new AllWarp());
        this.Register(25, new Use());
        this.Register(26, new Youtube());
        this.Register(27, new OldFoot());
        this.Register(28, new HidePyramide());
        this.Register(29, new ConfigBot());
        this.Register(30, new FastWalk());
        this.Register(31, new StartQuestion());
        this.Register(32, new StopQuestion());
        this.Register(33, new RoomYouTube());
        this.Register(34, new Kick());
        this.Register(35, new Coords());
        this.Register(36, new HandItem());
        this.Register(37, new Enable());
        this.Register(39, new About());
        this.Register(40, new ForceRot());
        this.Register(41, new EmptyItems());
        this.Register(42, new Follow());
        this.Register(43, new MoonWalk());
        this.Register(44, new Push());
        this.Register(45, new Pull());
        this.Register(46, new Mimic());
        this.Register(47, new Sit());
        this.Register(48, new Lay());
        this.Register(49, new Transf());
        this.Register(50, new TransfStop());
        this.Register(51, new DisableExchange());
        this.Register(52, new DisableFriendRequests());
        this.Register(53, new GiveItem());
        this.Register(54, new FaceLess());
        this.Register(55, new EmptyPets());
        this.Register(56, new Construit());
        this.Register(57, new ConstruitStop());
        this.Register(59, new EmptyBots());
        this.Register(60, new DisableFollow());
        this.Register(61, new InfoSuperWired());
        this.Register(62, new RockPaperScissors());
        this.Register(63, new Mazo());
        this.Register(65, new UseStop());
        this.Register(66, new GunFire());
        this.Register(67, new Cac());
        this.Register(68, new Pan());
        this.Register(69, new Prison());
        this.Register(71, new Emblem());
        this.Register(72, new GiveMoney());
        this.Register(73, new TransfBig());
        this.Register(74, new TransfLittle());
        this.Register(75, new ForceOpenGift());
        this.Register(76, new CloseDice());
        this.Register(77, new DND());
        this.Register(78, new Dance());
        this.Register(158, new ConvertMagot());
        this.Register(79, new UserInfo());
        this.Register(80, new Info());
        this.Register(81, new FaceWalk());
        this.Register(82, new VipProtect());
        this.Register(83, new Premium());
        this.Register(84, new TransfBot());
        this.Register(85, new RandomLook());
        this.Register(86, new GameTime());
        this.Register(159, new Balayette());
        this.Register(160, new Hug());
        this.Register(161, new Laser());
        this.Register(162, new Nuke());
        this.Register(163, new Slime());
        this.Register(164, new Tomato());
        this.Register(165, new Tied());
        this.Register(87, new StaffAlert());
        this.Register(88, new StaffsOnline());
        this.Register(89, new RoomAlert());
        this.Register(90, new Invisible());
        this.Register(91, new Ban());
        this.Register(92, new Disconnect());
        this.Register(93, new SuperBan());
        this.Register(94, new RoomKick());
        this.Register(95, new Mute());
        this.Register(96, new UnMute());
        this.Register(97, new Alert());
        this.Register(98, new StaffKick());
        this.Register(99, new DeleteMission());
        this.Register(100, new Summon());
        this.Register(101, new BanIP());
        this.Register(102, new TeleportStaff());
        this.Register(103, new WarpStaff());
        this.Register(104, new DisableWhispers());
        this.Register(105, new ForceFlagUser());
        this.Register(106, new KickBan());
        this.Register(107, new GiveLot());
        this.Register(108, new NotifTop());
        this.Register(169, new ShowInventory());
        this.Register(109, new DisabledAutoGame());
        this.Register(110, new RoomBadge());
        this.Register(111, new GiveBadge());
        this.Register(112, new ForceEnable());
        this.Register(113, new RemoveBadge());
        this.Register(114, new RoomEnable());
        this.Register(115, new UnloadRoom());
        this.Register(116, new ShowGuide());
        this.Register(117, new DuplicateRoom());
        this.Register(118, new SuperBot());
        this.Register(119, new PlaySoundRoom());
        this.Register(120, new StopSoundRoom());
        this.Register(122, new RoomEffect());
        this.Register(123, new ForceSit());
        this.Register(124, new Give());
        this.Register(125, new HotelAlert());
        this.Register(126, new MassBadge());
        this.Register(128, new AddFilter());
        this.Register(129, new EventAlert());
        this.Register(130, new ForceControlUser());
        this.Register(131, new MakeSay());
        this.Register(132, new ForceMimic());
        this.Register(133, new AddPhoto());
        this.Register(134, new Update());
        this.Register(135, new AllWhisper());
        this.Register(136, new AllIgnore());
        this.Register(137, new StartGameJD());
        this.Register(138, new AllAroundMe());
        this.Register(139, new AllEyesOnMe());
        this.Register(140, new RoomDance());
        this.Register(141, new ForceTransf());
        this.Register(142, new ForceEnableUser());
        this.Register(143, new TransfBot());
        this.Register(144, new AllFriends());
        this.Register(145, new RegenMap());
        this.Register(146, new ShutDown());
        this.Register(148, new ExtraBox());
        this.Register(149, new RoomSell());
        this.Register(150, new RoomBuy());
        this.Register(151, new RoomRemoveSell());
        this.Register(152, new LoadRoomItems());
        this.Register(153, new RegenLTD());
        this.Register(154, new SummonAll());
        this.Register(155, new LootboxInfo());
        this.Register(156, new UnloadEmptyRooms());
        this.Register(166, new GiveBanner());
        this.Register(167, new RemoveBanner());
        this.Register(168, new RoomBanner());
        this.Register(170, new ChatGTP());
        this.Register(171, new TransfertRoom());
        this.Register(172, new DeleteGroup());
        this.Register(173, new Turn());
        this.Register(175, new SuperPush());
        this.Register(176, new ChatAudio());
        this.Register(177, new ChatToSpeech());
        this.Register(178, new ChatToSpeechElevenlabs());
    }
}
