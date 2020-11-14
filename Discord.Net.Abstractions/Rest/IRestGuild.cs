using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Discord.Audio;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestGuild" />
    public interface IRestGuild : IGuild, IUpdateable
    {
        /// <inheritdoc cref="RestGuild.Emotes" />
        new IReadOnlyCollection<IGuildEmote> Emotes { get; }

        /// <inheritdoc cref="RestGuild.Roles" />
        new IReadOnlyCollection<IRestRole> Roles { get; }

        /// <inheritdoc cref="RestGuild.EveryoneRole" />
        new IRestRole EveryoneRole { get; }

        /// <inheritdoc cref="RestGuild.DefaultChannelId" />
        [Obsolete("DefaultChannelId is deprecated, use GetDefaultChannelAsync")]
        new ulong DefaultChannelId { get; }

        /// <inheritdoc cref="RestGuild.AddGuildUserAsync(ulong, string, Action{AddGuildUserProperties}, RequestOptions)" />
        new Task<IRestGuildUser> AddGuildUserAsync(ulong id, string accessToken, Action<AddGuildUserProperties> func = null, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.CreateCategoryChannelAsync(string, Action{GuildChannelProperties}, RequestOptions)" />
        Task<IRestCategoryChannel> CreateCategoryChannelAsync(string name, Action<GuildChannelProperties> func = null, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.CreateEmoteAsync(string, Image, Optional{IEnumerable{IRole}}, RequestOptions)" />
        new Task<IGuildEmote> CreateEmoteAsync(string name, Image image, Optional<IEnumerable<IRole>> roles = default, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.CreateIntegrationAsync(ulong, string, RequestOptions)" />
        new Task<IRestGuildIntegration> CreateIntegrationAsync(ulong id, string type, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.CreateRoleAsync(string, GuildPermissions?, Color?, bool, RequestOptions)" />
        new Task<IRestRole> CreateRoleAsync(string name, GuildPermissions? permissions = null, Color? color = null, bool isHoisted = false, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.CreateTextChannelAsync(string, Action{TextChannelProperties}, RequestOptions)" />
        new Task<IRestTextChannel> CreateTextChannelAsync(string name, Action<TextChannelProperties> func = null, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.CreateVoiceChannelAsync(string, Action{VoiceChannelProperties}, RequestOptions)" />
        new Task<IRestVoiceChannel> CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties> func = null, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetAFKChannelAsync(RequestOptions)" />
        Task<IRestVoiceChannel> GetAFKChannelAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetAuditLogsAsync(int, RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IRestAuditLogEntry>> GetAuditLogsAsync(int limit, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetBanAsync(ulong, RequestOptions)" />
        new Task<IBan> GetBanAsync(ulong userId, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetBanAsync(IUser, RequestOptions)" />
        new Task<IBan> GetBanAsync(IUser user, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetBansAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IBan>> GetBansAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetCategoryChannelsAsync(RequestOptions)" />
        Task<IReadOnlyCollection<IRestCategoryChannel>> GetCategoryChannelsAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetChannelAsync(ulong, RequestOptions)" />
        Task<IRestGuildChannel> GetChannelAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetChannelsAsync(RequestOptions)" />
        Task<IReadOnlyCollection<IRestGuildChannel>> GetChannelsAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetCurrentUserAsync(RequestOptions)" />
        Task<IRestGuildUser> GetCurrentUserAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetDefaultChannelAsync(RequestOptions)" />
        Task<IRestTextChannel> GetDefaultChannelAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetEmbedChannelAsync(RequestOptions)" />
        Task<IRestGuildChannel> GetEmbedChannelAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetEmoteAsync(ulong, RequestOptions)" />
        new Task<IGuildEmote> GetEmoteAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetIntegrationsAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestGuildIntegration>> GetIntegrationsAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetInvitesAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestInviteMetadata>> GetInvitesAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetOwnerAsync(RequestOptions)" />
        Task<IRestGuildUser> GetOwnerAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetRole(ulong)" />
        new IRestRole GetRole(ulong id);

        /// <inheritdoc cref="RestGuild.GetSystemChannelAsync(RequestOptions)" />
        Task<IRestTextChannel> GetSystemChannelAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetTextChannelAsync(ulong, RequestOptions)" />
        Task<IRestTextChannel> GetTextChannelAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetTextChannelsAsync(RequestOptions)" />
        Task<IReadOnlyCollection<IRestTextChannel>> GetTextChannelsAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetUserAsync(ulong, RequestOptions)" />
        Task<IRestGuildUser> GetUserAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetUsersAsync(RequestOptions)" />
        IAsyncEnumerable<IReadOnlyCollection<IRestGuildUser>> GetUsersAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetVanityInviteAsync(RequestOptions)" />
        new Task<IRestInviteMetadata> GetVanityInviteAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetVoiceChannelAsync(ulong, RequestOptions)" />
        Task<IRestVoiceChannel> GetVoiceChannelAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetVoiceChannelsAsync(RequestOptions)" />
        Task<IReadOnlyCollection<IRestVoiceChannel>> GetVoiceChannelsAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetVoiceRegionsAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetWebhookAsync(ulong, RequestOptions)" />
        new Task<IRestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.GetWebhooksAsync(RequestOptions)" />
        new Task<IReadOnlyCollection<IRestWebhook>> GetWebhooksAsync(RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.ModifyEmoteAsync(GuildEmote, Action{EmoteProperties}, RequestOptions)" />
        Task<IGuildEmote> ModifyEmoteAsync(IGuildEmote emote, Action<EmoteProperties> func, RequestOptions options = null);

        /// <inheritdoc cref="RestGuild.DeleteEmoteAsync(GuildEmote, RequestOptions)" />
        Task DeleteEmoteAsync(IGuildEmote emote, RequestOptions options = null);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestGuild"/>, through the <see cref="IRestGuild"/> interface.
    /// </summary>
    internal class RestGuildAbstraction : IRestGuild
    {
        /// <summary>
        /// Constructs a new <see cref="RestGuildAbstraction"/> around an existing <see cref="Rest.RestGuild"/>.
        /// </summary>
        /// <param name="restGuild">The value to use for <see cref="Rest.RestGuild"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restGuild"/>.</exception>
        public RestGuildAbstraction(RestGuild restGuild)
        {
            RestGuild = restGuild ?? throw new ArgumentNullException(nameof(restGuild));
        }

        /// <inheritdoc />
        public ulong? AFKChannelId
            => RestGuild.AFKChannelId;

        /// <inheritdoc />
        public int AFKTimeout
            => RestGuild.AFKTimeout;

        /// <inheritdoc />
        public ulong? ApplicationId
            => RestGuild.ApplicationId;

        /// <inheritdoc />
        public IAudioClient AudioClient
            => (RestGuild as IGuild).AudioClient;

        /// <inheritdoc />
        public bool Available
            => (RestGuild as IGuild).Available;

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => RestGuild.CreatedAt;

        /// <inheritdoc />
        public ulong DefaultChannelId
            => (RestGuild as IGuild).DefaultChannelId;

        /// <inheritdoc />
        public DefaultMessageNotifications DefaultMessageNotifications
            => RestGuild.DefaultMessageNotifications;

        /// <inheritdoc />
        public ulong? EmbedChannelId
            => RestGuild.EmbedChannelId;

        /// <inheritdoc />
        public IReadOnlyCollection<IGuildEmote> Emotes
            => RestGuild.Emotes
                .Select(GuildEmoteAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        IReadOnlyCollection<GuildEmote> IGuild.Emotes
            => (RestGuild as IGuild).Emotes;

        /// <inheritdoc />
        public IRestRole EveryoneRole
            => RestGuild.EveryoneRole
                .Abstract();

        /// <inheritdoc />
        IRole IGuild.EveryoneRole
            => (RestGuild as IGuild).EveryoneRole
                .Abstract();

        /// <inheritdoc />
        public ExplicitContentFilterLevel ExplicitContentFilter
            => RestGuild.ExplicitContentFilter;

        /// <inheritdoc />
        public IReadOnlyCollection<string> Features
            => (RestGuild as IGuild).Features;

        /// <inheritdoc />
        public string IconId
            => RestGuild.IconId;

        /// <inheritdoc />
        public string IconUrl
            => RestGuild.IconUrl;

        /// <inheritdoc />
        public ulong Id
            => RestGuild.Id;

        /// <inheritdoc />
        public bool IsEmbeddable
            => RestGuild.IsEmbeddable;

        /// <inheritdoc />
        public MfaLevel MfaLevel
            => RestGuild.MfaLevel;

        /// <inheritdoc />
        public string Name
            => RestGuild.Name;

        /// <inheritdoc />
        public ulong OwnerId
            => RestGuild.OwnerId;

        /// <inheritdoc />
        public IReadOnlyCollection<IRestRole> Roles
            => RestGuild.Roles
                .Select(RestRoleAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        IReadOnlyCollection<IRole> IGuild.Roles
            => (RestGuild as IGuild).Roles
                .Select(RoleAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public string SplashId
            => RestGuild.SplashId;

        /// <inheritdoc />
        public string SplashUrl
            => RestGuild.SplashUrl;

        /// <inheritdoc />
        public ulong? SystemChannelId
            => RestGuild.SystemChannelId;

        /// <inheritdoc />
        public VerificationLevel VerificationLevel
            => RestGuild.VerificationLevel;

        /// <inheritdoc />
        public string VoiceRegionId
            => RestGuild.VoiceRegionId;

        /// <inheritdoc />
        public Task AddBanAsync(ulong userId, int pruneDays = 0, string reason = null, RequestOptions options = null)
            => RestGuild.AddBanAsync(userId, pruneDays, reason, options);

        /// <inheritdoc />
        public Task AddBanAsync(IUser user, int pruneDays = 0, string reason = null, RequestOptions options = null)
            => RestGuild.AddBanAsync(user, pruneDays, reason, options);

        /// <inheritdoc />
        public async Task<IRestGuildUser> AddGuildUserAsync(ulong id, string accessToken, Action<AddGuildUserProperties> func = null, RequestOptions options = null)
            => (await RestGuild.AddGuildUserAsync(id, accessToken, func, options))
                .Abstract();

        /// <inheritdoc />
        async Task<IGuildUser> IGuild.AddGuildUserAsync(ulong userId, string accessToken, Action<AddGuildUserProperties> func, RequestOptions options)
            => (await (RestGuild as IGuild).AddGuildUserAsync(userId, accessToken, func, options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IRestCategoryChannel> CreateCategoryChannelAsync(string name, Action<GuildChannelProperties> func = null, RequestOptions options = null)
            => (await RestGuild.CreateCategoryChannelAsync(name, func, options))
                .Abstract();

        /// <inheritdoc />
        public async Task<ICategoryChannel> CreateCategoryAsync(string name, Action<GuildChannelProperties> func = null, RequestOptions options = null)
            => (await (RestGuild as IGuild).CreateCategoryAsync(name, func, options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IGuildEmote> CreateEmoteAsync(string name, Image image, Optional<IEnumerable<IRole>> roles = default, RequestOptions options = null)
            => (await RestGuild.CreateEmoteAsync(name, image, roles, options))
                .Abstract();

        /// <inheritdoc />
        Task<GuildEmote> IGuild.CreateEmoteAsync(string name, Image image, Optional<IEnumerable<IRole>> roles, RequestOptions options)
            => (RestGuild as IGuild).CreateEmoteAsync(name, image, roles, options);

        /// <inheritdoc />
        public async Task<IRestGuildIntegration> CreateIntegrationAsync(ulong id, string type, RequestOptions options = null)
            => (await RestGuild.CreateIntegrationAsync(id, type, options))
                .Abstract();

        /// <inheritdoc />
        async Task<IGuildIntegration> IGuild.CreateIntegrationAsync(ulong id, string type, RequestOptions options) =>
            (await RestGuild.CreateIntegrationAsync(id, type, options)).Abstract();

        /// <inheritdoc />
        public async Task<IRestRole> CreateRoleAsync(string name, GuildPermissions? permissions = null, Color? color = null, bool isHoisted = false, RequestOptions options = null)
            => (await RestGuild.CreateRoleAsync(name, permissions, color, isHoisted, options))
                .Abstract();

        Task<IRole> IGuild.CreateRoleAsync(string name, GuildPermissions? permissions, Color? color, bool isHoisted,
            RequestOptions options) => (RestGuild as IGuild).CreateRoleAsync(name, permissions, color, isHoisted, options);

        public async Task<IRole> CreateRoleAsync(string name, GuildPermissions? permissions = null, Color? color = null, bool isHoisted = false,
            bool isMentionable = false, RequestOptions options = null)
            => (await(RestGuild as IGuild).CreateRoleAsync(name, permissions, color, isHoisted, options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IRestTextChannel> CreateTextChannelAsync(string name, Action<TextChannelProperties> func = null, RequestOptions options = null)
            => (await RestGuild.CreateTextChannelAsync(name, func, options))
                .Abstract();

        /// <inheritdoc />
        async Task<ITextChannel> IGuild.CreateTextChannelAsync(string name, Action<TextChannelProperties> func, RequestOptions options)
            => (await (RestGuild as IGuild).CreateTextChannelAsync(name, func, options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IRestVoiceChannel> CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties> func = null, RequestOptions options = null)
            => (await RestGuild.CreateVoiceChannelAsync(name, func, options))
                .Abstract();

        /// <inheritdoc />
        async Task<IVoiceChannel> IGuild.CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties> func, RequestOptions options)
            => (await (RestGuild as IGuild).CreateVoiceChannelAsync(name, func, options))
                .Abstract();

        /// <inheritdoc />
        public Task DeleteAsync(RequestOptions options = null)
            => RestGuild.DeleteAsync(options);

        /// <inheritdoc />
        public Task DeleteEmoteAsync(IGuildEmote emote, RequestOptions options = null)
            => RestGuild.DeleteEmoteAsync(emote.Unabstract(), options);

        /// <inheritdoc />
        public Task DeleteEmoteAsync(GuildEmote emote, RequestOptions options = null)
            => RestGuild.DeleteEmoteAsync(emote, options);

        /// <inheritdoc />
        public Task DownloadUsersAsync()
            => (RestGuild as IGuild).DownloadUsersAsync();

        public Task<int> PruneUsersAsync(int days = 30, bool simulate = false, RequestOptions options = null,
            IEnumerable<ulong> includeRoleIds = null) =>
            RestGuild.PruneUsersAsync(days, simulate, options, includeRoleIds);

        /// <inheritdoc />
        public async Task<IRestVoiceChannel> GetAFKChannelAsync(RequestOptions options = null)
            => (await RestGuild.GetAFKChannelAsync(options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IVoiceChannel> GetAFKChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetAFKChannelAsync(mode, options))
                ?.Abstract();

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestAuditLogEntry>> GetAuditLogsAsync(int limit, RequestOptions options = null)
            => RestGuild.GetAuditLogsAsync(limit, options)
                .Select(x => x
                    .Select(RestAuditLogEntryAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IAuditLogEntry>> GetAuditLogsAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetAuditLogsAsync(limit, mode, options))
                .Select(AuditLogEntryAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IBan> GetBanAsync(ulong userId, RequestOptions options = null)
            => await RestGuild.GetBanAsync(userId, options);

        /// <inheritdoc />
        public async Task<IBan> GetBanAsync(IUser user, RequestOptions options = null)
            => await RestGuild.GetBanAsync(user, options);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IBan>> GetBansAsync(RequestOptions options = null)
            => await RestGuild.GetBansAsync(options);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ICategoryChannel>> GetCategoriesAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetCategoriesAsync(mode, options))
                .Select(CategoryChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestCategoryChannel>> GetCategoryChannelsAsync(RequestOptions options = null)
            => (await RestGuild.GetCategoryChannelsAsync(options))
                .Select(RestCategoryChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IRestGuildChannel> GetChannelAsync(ulong id, RequestOptions options = null)
            => (await RestGuild.GetChannelAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IGuildChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetChannelAsync(id, mode, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestGuildChannel>> GetChannelsAsync(RequestOptions options = null)
            => (await RestGuild.GetChannelsAsync(options))
                .Select(RestGuildChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IGuildChannel>> GetChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetChannelsAsync(mode, options))
                .Select(GuildChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IRestGuildUser> GetCurrentUserAsync(RequestOptions options = null)
            => (await RestGuild.GetCurrentUserAsync(options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IGuildUser> GetCurrentUserAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetCurrentUserAsync(mode, options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IRestTextChannel> GetDefaultChannelAsync(RequestOptions options = null)
            => (await RestGuild.GetDefaultChannelAsync(options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<ITextChannel> GetDefaultChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetDefaultChannelAsync(mode, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IRestGuildChannel> GetEmbedChannelAsync(RequestOptions options = null)
            => (await RestGuild.GetEmbedChannelAsync(options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IGuildChannel> GetEmbedChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetEmbedChannelAsync(mode, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IGuildEmote> GetEmoteAsync(ulong id, RequestOptions options = null)
            => (await RestGuild.GetEmoteAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        Task<GuildEmote> IGuild.GetEmoteAsync(ulong id, RequestOptions options)
            => (RestGuild as IGuild).GetEmoteAsync(id, options);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestGuildIntegration>> GetIntegrationsAsync(RequestOptions options = null)
            => (await RestGuild.GetIntegrationsAsync(options))
                .Select(RestGuildIntegrationAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        async Task<IReadOnlyCollection<IGuildIntegration>> IGuild.GetIntegrationsAsync(RequestOptions options)
            => (await (RestGuild as IGuild).GetIntegrationsAsync(options))
                .Select(GuildIntegrationAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
            => (await RestGuild.GetInvitesAsync(options))
                .Select(RestInviteMetadataAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        async Task<IReadOnlyCollection<IInviteMetadata>> IGuild.GetInvitesAsync(RequestOptions options)
            => (await (RestGuild as IGuild).GetInvitesAsync(options))
                .Select(InviteMetadataAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IRestGuildUser> GetOwnerAsync(RequestOptions options = null)
            => (await RestGuild.GetOwnerAsync(options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IGuildUser> GetOwnerAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetOwnerAsync(mode, options))
                .Abstract();

        /// <inheritdoc />
        public IRestRole GetRole(ulong id)
            => RestGuild.GetRole(id)
                ?.Abstract();

        /// <inheritdoc />
        IRole IGuild.GetRole(ulong id)
            => (RestGuild as IGuild).GetRole(id)
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IRestTextChannel> GetSystemChannelAsync(RequestOptions options = null)
            => (await RestGuild.GetSystemChannelAsync(options))
                .Abstract();

        /// <inheritdoc />
        public async Task<ITextChannel> GetSystemChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetSystemChannelAsync(mode, options))
                .Abstract();

        /// <inheritdoc />
        public async Task<IRestTextChannel> GetTextChannelAsync(ulong id, RequestOptions options = null)
            => (await RestGuild.GetTextChannelAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<ITextChannel> GetTextChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetTextChannelAsync(id, mode, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestTextChannel>> GetTextChannelsAsync(RequestOptions options = null)
            => (await RestGuild.GetTextChannelsAsync(options))
                .Select(RestTextChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public Task<IReadOnlyCollection<ITextChannel>> GetTextChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (RestGuild as IGuild).GetTextChannelsAsync(mode, options);

        /// <inheritdoc />
        public async Task<IRestGuildUser> GetUserAsync(ulong id, RequestOptions options = null)
            => (await RestGuild.GetUserAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IGuildUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetUserAsync(id, mode, options))
                ?.Abstract();

        /// <inheritdoc />
        public IAsyncEnumerable<IReadOnlyCollection<IRestGuildUser>> GetUsersAsync(RequestOptions options = null)
            => RestGuild.GetUsersAsync(options)
                .Select(x => x
                    .Select(RestGuildUserAbstractionExtensions.Abstract)
                    .ToArray());

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IGuildUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetUsersAsync(mode, options))
                .Select(GuildUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IRestInviteMetadata> GetVanityInviteAsync(RequestOptions options = null)
            => (await RestGuild.GetVanityInviteAsync(options))
                ?.Abstract();

        /// <inheritdoc />
        async Task<IInviteMetadata> IGuild.GetVanityInviteAsync(RequestOptions options)
            => (await (RestGuild as IGuild).GetVanityInviteAsync(options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IRestVoiceChannel> GetVoiceChannelAsync(ulong id, RequestOptions options = null)
            => (await RestGuild.GetVoiceChannelAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IVoiceChannel> GetVoiceChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetVoiceChannelAsync(id, mode, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestVoiceChannel>> GetVoiceChannelsAsync(RequestOptions options = null)
            => (await RestGuild.GetVoiceChannelsAsync(options))
                .Select(RestVoiceChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IVoiceChannel>> GetVoiceChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => (await (RestGuild as IGuild).GetVoiceChannelsAsync(mode, options))
                .Select(VoiceChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
            => (await RestGuild.GetVoiceRegionsAsync(options))
                .Select(RestVoiceRegionAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        async Task<IReadOnlyCollection<IVoiceRegion>> IGuild.GetVoiceRegionsAsync(RequestOptions options)
            => (await (RestGuild as IGuild).GetVoiceRegionsAsync(options))
                .Select(VoiceRegionAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public async Task<IRestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
            => (await RestGuild.GetWebhookAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        async Task<IWebhook> IGuild.GetWebhookAsync(ulong id, RequestOptions options)
            => (await (RestGuild as IGuild).GetWebhookAsync(id, options))
                ?.Abstract();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IRestWebhook>> GetWebhooksAsync(RequestOptions options = null)
            => (await RestGuild.GetWebhooksAsync(options))
                .Select(RestWebhookAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        async Task<IReadOnlyCollection<IWebhook>> IGuild.GetWebhooksAsync(RequestOptions options)
            => (await (RestGuild as IGuild).GetWebhooksAsync(options))
                .Select(WebhookAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public Task LeaveAsync(RequestOptions options = null)
            => RestGuild.LeaveAsync(options);

        /// <inheritdoc />
        public async Task<IGuildEmote> ModifyEmoteAsync(IGuildEmote emote, Action<EmoteProperties> func, RequestOptions options = null)
            => (await RestGuild.ModifyEmoteAsync(emote.Unabstract(), func, options))
                .Abstract();

        /// <inheritdoc />
        public Task ModifyAsync(Action<GuildProperties> func, RequestOptions options = null)
            => RestGuild.ModifyAsync(func, options);

        /// <inheritdoc />
        public Task ModifyEmbedAsync(Action<GuildEmbedProperties> func, RequestOptions options = null)
            => RestGuild.ModifyEmbedAsync(func, options);

        /// <inheritdoc />
        public Task<GuildEmote> ModifyEmoteAsync(GuildEmote emote, Action<EmoteProperties> func, RequestOptions options = null)
            => (RestGuild as IGuild).ModifyEmoteAsync(emote, func, options);

        /// <inheritdoc />
        public Task RemoveBanAsync(ulong userId, RequestOptions options = null)
            => RestGuild.RemoveBanAsync(userId, options);

        /// <inheritdoc />
        public Task RemoveBanAsync(IUser user, RequestOptions options = null)
            => RestGuild.RemoveBanAsync(user, options);

        /// <inheritdoc />
        public Task ReorderChannelsAsync(IEnumerable<ReorderChannelProperties> args, RequestOptions options = null)
            => RestGuild.ReorderChannelsAsync(args, options);

        /// <inheritdoc />
        public Task ReorderRolesAsync(IEnumerable<ReorderRoleProperties> args, RequestOptions options = null)
            => RestGuild.ReorderRolesAsync(args, options);

        /// <inheritdoc />
        public Task<IReadOnlyCollection<RestGuildUser>> SearchUsersAsync(string query, int limit = 1000, RequestOptions options = null)
            => RestGuild.SearchUsersAsync(query, limit, options);

        /// <inheritdoc />
        Task<IReadOnlyCollection<IGuildUser>> IGuild.SearchUsersAsync(string query, int limit, CacheMode mode, RequestOptions options)
            => ((IGuild)RestGuild).SearchUsersAsync(query, limit, mode, options);

        /// <inheritdoc />
        public Task UpdateAsync(RequestOptions options = null)
            => RestGuild.UpdateAsync(options);

        /// <inheritdoc cref="RestGuild.ToString" />
        public override string ToString()
            => RestGuild.ToString();

        public async Task<IReadOnlyCollection<IAuditLogEntry>> GetAuditLogsAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null, ulong? beforeId = null, ulong? userId = null, ActionType? actionType = null)
            => (await RestGuild.GetAuditLogsAsync(limit, options, beforeId, userId, actionType).FlattenAsync())
            .ToArray();

        /// <summary>
        /// The existing <see cref="Rest.RestGuild"/> being abstracted.
        /// </summary>
        protected RestGuild RestGuild { get; }

        public PremiumTier PremiumTier => RestGuild.PremiumTier;

        public string BannerId => RestGuild.BannerId;

        public string BannerUrl => RestGuild.BannerUrl;

        public string VanityURLCode => RestGuild.VanityURLCode;

        public SystemChannelMessageDeny SystemChannelFlags => RestGuild.SystemChannelFlags;

        public string Description => RestGuild.Description;

        public int PremiumSubscriptionCount => RestGuild.PremiumSubscriptionCount;

        public string PreferredLocale => RestGuild.PreferredLocale;

        public CultureInfo PreferredCulture => RestGuild.PreferredCulture;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestGuild"/> objects.
    /// </summary>
    internal static class RestGuildAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestGuild"/> to an abstracted <see cref="IRestGuild"/> value.
        /// </summary>
        /// <param name="restGuild">The existing <see cref="RestGuild"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restGuild"/>.</exception>
        /// <returns>An <see cref="IRestGuild"/> that abstracts <paramref name="restGuild"/>.</returns>
        public static IRestGuild Abstract(this RestGuild restGuild)
            => new RestGuildAbstraction(restGuild);
    }
}
