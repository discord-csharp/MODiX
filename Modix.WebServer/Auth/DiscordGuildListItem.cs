namespace Modix.WebServer.Auth
{
    public class DiscordGuildListItem
    {
        public bool Owner { get; set; }
        public int Permissions { get; set; }
        public string Icon { get; set; }
        public ulong Id { get; set; }
        public string Name { get; set; }
    }
}