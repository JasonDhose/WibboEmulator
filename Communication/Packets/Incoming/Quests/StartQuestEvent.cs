namespace WibboEmulator.Communication.Packets.Incoming.Quests;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Quests;

internal sealed class StartQuestEvent : IPacketEvent
{
    public double Delay => 0;

    public void Parse(GameClient Session, ClientPacket packet)
    {
        var questId = packet.PopInt();

        QuestManager.ActivateQuest(Session, questId);
    }
}
