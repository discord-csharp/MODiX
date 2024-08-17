namespace Modix.Web.Models;

public class SessionState
{
    public ulong SelectedGuild { get; set; }
    public ulong CurrentUserId { get; set; }
    public bool ShowDeletedInfractions { get; set; }
    public bool ShowInfractionState { get; set; }
    public bool ShowInactivePromotions { get; set; }
    public bool UseDarkMode { get; set; }
}
