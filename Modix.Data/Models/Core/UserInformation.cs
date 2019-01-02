using System;
using System.Collections.Generic;

using Discord;

namespace Modix.Data.Models.Core
{
    public class UserInformation
    {
        public IActivity Activity { get; set; }

        public string AvatarId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public string Discriminator { get; set; }

        public ushort DiscriminatorValue { get; set; }

        public DateTimeOffset? FirstSeen { get; set; }

        public GuildPermissions GuildPermissions { get; set; }

        public IGuild Guild { get; set; }

        public ulong? GuildId { get; set; }

        public ulong Id { get; set; }

        public bool IsBot { get; set; }
        
        public bool IsDeafened { get; set; }

        public bool IsMuted { get; set; }

        public bool IsSelfDeafened { get; set; }

        public bool IsSelfMuted { get; set; }

        public bool IsSuppressed { get; set; }

        public bool IsWebhook { get; set; }

        public DateTimeOffset? JoinedAt { get; set; }

        public DateTimeOffset? LastSeen { get; set; }

        public string Mention { get; set; }

        public string Nickname { get; set; }

        public IReadOnlyCollection<ulong> RoleIds { get; set; }

        public UserStatus Status { get; set; }

        public string Username { get; set; }

        public IVoiceChannel VoiceChannel { get; set; }

        public string VoiceSessionId { get; set; }

        public UserInformation WithIEntityData(IEntity<ulong> user)
        {
            if (user is null)
                return this;

            Id = user.Id;

            return this;
        }

        public UserInformation WithISnowflakeEntityData(ISnowflakeEntity user)
        {
            if (user is null)
                return this;
            
            CreatedAt = user.CreatedAt;

            return WithIEntityData(user);
        }

        public UserInformation WithIMentionableData(IMentionable user)
        {
            if (user is null)
                return this;

            Mention = user.Mention;

            return this;
        }

        public UserInformation WithIPresenceData(IPresence user)
        {
            if (user is null)
                return this;

            Activity = user.Activity;
            Status = user.Status;

            return this;
        }

        public UserInformation WithIUserData(IUser user)
        {
            if (user is null)
                return this;

            Activity = user.Activity;
            AvatarId = user.AvatarId;
            Discriminator = user.Discriminator;
            DiscriminatorValue = user.DiscriminatorValue;
            IsBot = user.IsBot;
            IsWebhook = user.IsWebhook;
            Status = user.Status;
            Username = user.Username;

            return WithISnowflakeEntityData(user)
                .WithIMentionableData(user)
                .WithIPresenceData(user);
        }

        public UserInformation WithIVoiceStateData(IVoiceState user)
        {
            if (user is null)
                return this;

            IsDeafened = user.IsDeafened;
            IsMuted = user.IsMuted;
            IsSelfDeafened = user.IsSelfDeafened;
            IsSelfMuted = user.IsSelfMuted;
            IsSuppressed = user.IsSuppressed;
            VoiceChannel = user.VoiceChannel;
            VoiceSessionId = user.VoiceSessionId;

            return this;
        }

        public UserInformation WithIGuildUserData(IGuildUser user)
        {
            if (user is null)
                return this;

            JoinedAt = user.JoinedAt;
            Nickname = user.Nickname;
            GuildPermissions = user.GuildPermissions;
            Guild = user.Guild;
            GuildId = user.GuildId;
            RoleIds = user.RoleIds;

            return WithIUserData(user)
                .WithIVoiceStateData(user);
        }

        public UserInformation WithGuildUserSummaryData(GuildUserSummary user)
        {
            if (user is null)
                return this;

            Id = user.UserId;
            GuildId = user.GuildId;
            Username = user.Username;
            Discriminator = user.Discriminator;
            Nickname = user.Nickname;
            FirstSeen = user.FirstSeen;
            LastSeen = user.LastSeen;

            return this;
        }
    }
}
