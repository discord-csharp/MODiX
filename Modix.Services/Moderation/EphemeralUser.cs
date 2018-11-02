using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modix.Services.Moderation
{
    public class EphemeralUser : IGuildUser
    {
        public EphemeralUser(ulong userId, string name, IGuild guild)
        {
            Id = userId;
            Username = name;
            Guild = guild;
        }

        public DateTimeOffset? JoinedAt { get; private set; } = null;

        public string Nickname { get; private set; } = null;

        public GuildPermissions GuildPermissions { get; private set; } = new GuildPermissions();

        public IGuild Guild { get; private set; }

        public ulong GuildId => Guild.Id;

        public IReadOnlyCollection<ulong> RoleIds { get; private set; } = new List<ulong>();

        public string AvatarId { get; private set; } = null;

        public string Discriminator { get; private set; } = null;

        public ushort DiscriminatorValue { get; private set; } = 0;

        public bool IsBot { get; private set; } = false;

        public bool IsWebhook { get; private set; } = false;

        public string Username { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

        public ulong Id { get; private set; }

        public string Mention => MentionUtils.MentionUser(Id);

        public Game? Game { get; private set; } = null;

        public UserStatus Status { get; private set; } = UserStatus.Offline;

        public bool IsDeafened { get; private set; } = false;

        public bool IsMuted { get; private set; } = false;

        public bool IsSelfDeafened { get; private set; } = false;

        public bool IsSelfMuted { get; private set; } = false;

        public bool IsSuppressed { get; private set; } = false;

        public IVoiceChannel VoiceChannel { get; private set; } = null;

        public string VoiceSessionId { get; private set; } = null;

        public Task AddRoleAsync(IRole role, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            throw new NotImplementedException();
        }

        public Task<IDMChannel> GetOrCreateDMChannelAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public ChannelPermissions GetPermissions(IGuildChannel channel)
        {
            throw new NotImplementedException();
        }

        public Task KickAsync(string reason = null, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRoleAsync(IRole role, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }
    }
}
