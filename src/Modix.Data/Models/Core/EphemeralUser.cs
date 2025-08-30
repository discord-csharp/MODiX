using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modix.Data.Models.Core
{
    public class EphemeralUser : IGuildUser
    {
        private const string NotImplementedMessage = "This operation is invalid on untrackable users";

        public DateTimeOffset? JoinedAt { get; private set; }

        public string? Nickname { get; private set; }

        public string? GuildBannerHash { get; private set; }

        public GuildPermissions GuildPermissions { get; private set; } = new GuildPermissions();

        public IGuild? Guild { get; private set; }

        public ulong GuildId { get; private set; }

        public IReadOnlyCollection<ulong> RoleIds { get; private set; } = new List<ulong>();

        public string? AvatarId { get; private set; }

        public string? Discriminator { get; private set; }

        public ushort DiscriminatorValue { get; private set; }

        public bool IsBot { get; private set; }

        public bool IsWebhook { get; private set; }

        public string? Username { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

        public ulong Id { get; private set; }

        public string Mention => MentionUtils.MentionUser(Id);

        public Game? Game { get; private set; }

        public UserStatus Status { get; private set; } = UserStatus.Offline;

        public bool IsDeafened { get; private set; }

        public bool IsMuted { get; private set; }

        public bool IsSelfDeafened { get; private set; }

        public bool IsSelfMuted { get; private set; }

        public bool IsSuppressed { get; private set; }

        public IVoiceChannel? VoiceChannel { get; private set; }

        public string? VoiceSessionId { get; private set; }

        public DateTimeOffset? FirstSeen { get; private set; }

        public DateTimeOffset? LastSeen { get; private set; }

        public bool IsBanned { get; private set; }

        public string? BanReason { get; private set; }

        public DateTimeOffset? PremiumSince { get; private set; }

        public IReadOnlyCollection<ClientType>? ActiveClients { get; private set; }

        public IReadOnlyCollection<IActivity>? Activities { get; private set; }

        public bool IsStreaming { get; private set; }

        public bool? IsPending { get; private set; }

        public UserProperties? PublicFlags { get; private set; }

        public string? DisplayName { get; private set; }

        public string? DisplayAvatarId { get; private set; }

        public string? GuildAvatarId { get; private set; }

        public int Hierarchy { get; private set; }

        public DateTimeOffset? TimedOutUntil { get; private set; }

        public bool IsVideoing { get; private set; }

        public DateTimeOffset? RequestToSpeakTimestamp { get; private set; }

        public GuildUserFlags Flags { get; private set; }

        public string? GlobalName { get; private set; }

        public string? AvatarDecorationHash { get; private set; }

        public ulong? AvatarDecorationSkuId { get; private set; }

        public async Task AddRoleAsync(ulong roleId, RequestOptions? options = null)
            => await OnGuildUserOrThrowAsync(user => user.AddRoleAsync(roleId, options));

        public async Task AddRolesAsync(IEnumerable<ulong> roleIds, RequestOptions? options = null)
            => await OnGuildUserOrThrowAsync(user => user.AddRolesAsync(roleIds, options));

        public async Task RemoveRoleAsync(ulong roleId, RequestOptions? options = null)
            => await OnGuildUserOrThrowAsync(user => user.RemoveRoleAsync(roleId, options));

        public async Task RemoveRolesAsync(IEnumerable<ulong> roleIds, RequestOptions? options = null)
            => await OnGuildUserOrThrowAsync(user => user.RemoveRolesAsync(roleIds, options));

        public async Task AddRoleAsync(IRole role, RequestOptions? options = null)
        {
            await OnGuildUserOrThrowAsync(user => user.AddRoleAsync(role, options));
        }

        public async Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions? options = null)
        {
            await OnGuildUserOrThrowAsync(user => user.AddRolesAsync(roles, options));
        }

        public async Task<IDMChannel> GetOrCreateDMChannelAsync(RequestOptions? options = null)
        {
            return await OnGuildUserOrThrowAsync(user => user.CreateDMChannelAsync(options));
        }

        public ChannelPermissions GetPermissions(IGuildChannel channel)
        {
            return OnGuildUserOrThrow(user => user.GetPermissions(channel));
        }

        public string GetGuildBannerUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128) => throw new NotImplementedException();

        public async Task KickAsync(string? reason = null, RequestOptions? options = null)
        {
            await OnGuildUserOrThrowAsync(u => u.KickAsync(reason, options));
        }

        public async Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions? options = null)
        {
            await OnGuildUserOrThrowAsync(u => u.ModifyAsync(func, options));
        }

        public async Task RemoveRoleAsync(IRole role, RequestOptions? options = null)
        {
            await OnGuildUserOrThrowAsync(u => u.RemoveRoleAsync(role, options));
        }

        public async Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions? options = null)
        {
            await OnGuildUserOrThrowAsync(u => u.RemoveRolesAsync(roles, options));
        }

        public string? GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
            => _user?.GetAvatarUrl(format, size);

        public string? GetDefaultAvatarUrl()
            => _user?.GetDefaultAvatarUrl();

        public string GetGuildAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            return OnGuildUserOrThrow(u => u.GetGuildAvatarUrl(format, size));
        }

        public string GetDisplayAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            return OnGuildUserOrThrow(u => u.GetDisplayAvatarUrl(format, size));
        }

        public async Task SetTimeOutAsync(TimeSpan span, RequestOptions? options = null)
        {
            await OnGuildUserOrThrowAsync(u => u.SetTimeOutAsync(span, options));
        }

        public async Task RemoveTimeOutAsync(RequestOptions? options = null)
        {
            await OnGuildUserOrThrowAsync(u => u.RemoveTimeOutAsync(options));
        }

        public async Task<IDMChannel> CreateDMChannelAsync(RequestOptions? options = null)
        {
            return await OnGuildUserOrThrowAsync(u => u.CreateDMChannelAsync(options));
        }

        public EphemeralUser WithIEntityData(IEntity<ulong>? user)
        {
            if (user is null)
                return this;

            if (user.Id != default)
                Id = user.Id;

            return this;
        }

        public EphemeralUser WithISnowflakeEntityData(ISnowflakeEntity? user)
        {
            if (user is null)
                return this;

            if (user.CreatedAt != default)
                CreatedAt = user.CreatedAt;

            return WithIEntityData(user);
        }

        public EphemeralUser WithIPresenceData(IPresence? user)
        {
            if (user is null)
                return this;

            if (user.ActiveClients is not null)
                ActiveClients = user.ActiveClients;

            if (user.Activities is not null)
                Activities = user.Activities;

            if (user.Status != default)
                Status = user.Status;

            return this;
        }

        public EphemeralUser WithIUserData(IUser? user)
        {
            if (user is null)
                return this;

            _user = user;

            if (user.AvatarId != default)
                AvatarId = user.AvatarId;

            if (user.Discriminator != default)
                Discriminator = user.Discriminator;

            if (user.DiscriminatorValue != default)
                DiscriminatorValue = user.DiscriminatorValue;

            if (user.GlobalName != default)
                GlobalName = user.GlobalName;

            if (user.IsBot != default)
                IsBot = user.IsBot;

            if (user.IsWebhook != default)
                IsWebhook = user.IsWebhook;

            if (user.Status != default)
                Status = user.Status;

            if (user.Username != default)
                Username = user.Username;

            return WithISnowflakeEntityData(user)
                .WithIPresenceData(user);
        }

        public EphemeralUser WithIVoiceStateData(IVoiceState? user)
        {
            if (user is null)
                return this;

            if (user.IsDeafened != default)
                IsDeafened = user.IsDeafened;

            if (user.IsMuted != default)
                IsMuted = user.IsMuted;

            if (user.IsSelfDeafened != default)
                IsSelfDeafened = user.IsSelfDeafened;

            if (user.IsSelfMuted != default)
                IsSelfMuted = user.IsSelfMuted;

            if (user.IsSuppressed != default)
                IsSuppressed = user.IsSuppressed;

            if (user.IsVideoing != default)
                IsVideoing = user.IsVideoing;

            if (user.RequestToSpeakTimestamp != default)
                RequestToSpeakTimestamp = user.RequestToSpeakTimestamp;

            if (user.VoiceChannel != default)
                VoiceChannel = user.VoiceChannel;

            if (user.VoiceSessionId != default)
                VoiceSessionId = user.VoiceSessionId;

            return this;
        }

        public EphemeralUser WithIGuildUserData(IGuildUser? user)
        {
            if (user is null)
                return this;

            _guildUser = user;

            if (user.DisplayName != default)
                DisplayName = user.DisplayName;

            if (user.DisplayAvatarId != default)
                DisplayAvatarId = user.DisplayAvatarId;

            if (user.GuildAvatarId != default)
                GuildAvatarId = user.GuildAvatarId;

            if (user.Hierarchy != default)
                Hierarchy = user.Hierarchy;

            if (user.JoinedAt != default)
                JoinedAt = user.JoinedAt;

            if (user.Nickname != default)
                Nickname = user.Nickname;

            if (user.Flags != default)
                Flags = user.Flags;

            if (user.Guild != default)
                Guild = user.Guild;

            if (user.GuildId != default)
                GuildId = user.GuildId;

            if (!user.GuildPermissions.Equals(default(GuildPermissions)))
                GuildPermissions = user.GuildPermissions;

            if (user.RoleIds != default)
                RoleIds = user.RoleIds;

            if (user.TimedOutUntil != default)
                TimedOutUntil = user.TimedOutUntil;

            if (user.AvatarDecorationHash != default)
                AvatarDecorationHash = user.AvatarDecorationHash;

            if (user.AvatarDecorationSkuId != default)
                AvatarDecorationSkuId = user.AvatarDecorationSkuId;

            return WithIUserData(user)
                .WithIVoiceStateData(user);
        }

        public EphemeralUser WithGuildUserSummaryData(GuildUserSummary? user)
        {
            if (user is null)
                return this;

            if (user.UserId != default)
                Id = user.UserId;

            if (user.GuildId != default)
                GuildId = user.GuildId;

            if (user.Username != default)
                Username = user.Username;

            if (user.Discriminator != default)
                Discriminator = user.Discriminator;

            if (user.Nickname != default)
                Nickname = user.Nickname;

            if (user.FirstSeen != default)
                FirstSeen = user.FirstSeen;

            if (user.LastSeen != default)
                LastSeen = user.LastSeen;

            return this;
        }

        public EphemeralUser WithGuildContext(IGuild? guild)
        {
            if (guild is { })
            {
                Guild = guild;
                GuildId = guild.Id;
            }

            return this;
        }

        public EphemeralUser WithBanData(IBan? ban)
        {
            if (ban is null)
                return this;

            IsBanned = true;

            BanReason = ban.Reason;

            return this;
        }

        private async Task OnGuildUserOrThrowAsync(Func<IGuildUser, Task> action)
        {
            if (_guildUser == null)
            {
                throw new NotImplementedException(NotImplementedMessage);
            }

            await action.Invoke(_guildUser);
        }

        private async Task<T> OnGuildUserOrThrowAsync<T>(Func<IGuildUser, Task<T>> action)
        {
            if (_guildUser == null)
            {
                throw new NotImplementedException(NotImplementedMessage);
            }

            return await action.Invoke(_guildUser);
        }

        private T OnGuildUserOrThrow<T>(Func<IGuildUser, T> action)
        {
            if (_guildUser == null)
            {
                throw new NotImplementedException(NotImplementedMessage);
            }

            return action.Invoke(_guildUser);
        }

        public string GetAvatarDecorationUrl()
            => $"{DiscordConfig.CDNUrl}avatar-decoration-presets/{AvatarDecorationHash}.png";

        private IUser? _user;
        private IGuildUser? _guildUser;
    }
}
