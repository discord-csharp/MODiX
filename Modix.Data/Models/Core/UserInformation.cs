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

            if (user.Id != default)
                Id = user.Id;

            return this;
        }

        public UserInformation WithISnowflakeEntityData(ISnowflakeEntity user)
        {
            if (user is null)
                return this;

            if (user.CreatedAt != default)
                CreatedAt = user.CreatedAt;

            return WithIEntityData(user);
        }

        public UserInformation WithIMentionableData(IMentionable user)
        {
            if (user is null)
                return this;

            if (user.Mention != default)
                Mention = user.Mention;

            return this;
        }

        public UserInformation WithIPresenceData(IPresence user)
        {
            if (user is null)
                return this;

            if (user.Activity != default)
                Activity = user.Activity;

            if (user.Status != default)
                Status = user.Status;

            return this;
        }

        public UserInformation WithIUserData(IUser user)
        {
            if (user is null)
                return this;

            if (user.AvatarId != default)
                AvatarId = user.AvatarId;
            
            if (user.Discriminator != default)
                Discriminator = user.Discriminator;
            
            if (user.DiscriminatorValue != default)
                DiscriminatorValue = user.DiscriminatorValue;

            if (user.IsBot != default)
                IsBot = user.IsBot;

            if (user.IsWebhook != default)
                IsWebhook = user.IsWebhook;

            if (user.Status != default)
                Status = user.Status;

            if (user.Username != default)
                Username = user.Username;

            return WithISnowflakeEntityData(user)
                .WithIMentionableData(user)
                .WithIPresenceData(user);
        }

        public UserInformation WithIVoiceStateData(IVoiceState user)
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

            if (user.VoiceChannel != default)
                VoiceChannel = user.VoiceChannel;

            if (user.VoiceSessionId != default)
                VoiceSessionId = user.VoiceSessionId;

            return this;
        }

        public UserInformation WithIGuildUserData(IGuildUser user)
        {
            if (user is null)
                return this;

            if (user.JoinedAt != default)
                JoinedAt = user.JoinedAt;

            if (user.Nickname != default)
                Nickname = user.Nickname;

            if (user.Guild != default)
                Guild = user.Guild;

            if (user.GuildId != default)
                GuildId = user.GuildId;

            if (!user.GuildPermissions.Equals(default(GuildPermissions)))
                GuildPermissions = user.GuildPermissions;

            if (user.RoleIds != default)
                RoleIds = user.RoleIds;

            return WithIUserData(user)
                .WithIVoiceStateData(user);
        }

        public UserInformation WithGuildUserSummaryData(GuildUserSummary user)
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
    }
}
