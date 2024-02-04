namespace WibboEmulator.Games.Badges;

public class BadgeManager
{
    private readonly List<string> _notAllowed;

    public BadgeManager() => this._notAllowed = new List<string>();

    public void Init()
    {
        this._notAllowed.Clear();

        var badgeNotAllowed = WibboEnvironment.GetSettings().GetData<string>("badge.not.allowed");

        this._notAllowed.AddRange(badgeNotAllowed.Split(','));
    }

    public bool HaveNotAllowed(string badgeId)
    {
        if (this._notAllowed.Contains(badgeId))
        {
            return true;
        }

        if (badgeId.StartsWith("MRUN") || badgeId.StartsWith("WORLDRUNSAVE") || badgeId.StartsWith("ACH_"))
        {
            return true;
        }

        return false;
    }

    public List<string> GetNotAllowed() => this._notAllowed;
}
