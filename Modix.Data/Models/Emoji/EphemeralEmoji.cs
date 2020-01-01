using System;
using System.Collections.Generic;
using Discord;

namespace Modix.Data.Models.Emoji
{
    public sealed class EphemeralEmoji : ISnowflakeEntity, IEmote, IEquatable<EphemeralEmoji>
    {
        public ulong? Id
            => _emote?.Id;

        public DateTimeOffset? CreatedAt
            => _emote?.CreatedAt;

        public string? Name
            => _emote?.Name ?? _emoji?.Name;

        public bool Animated
            => _emote?.Animated ?? false;

        public string? Url
            => _emote?.Url;

        ulong IEntity<ulong>.Id
            => Id ?? throw new InvalidOperationException(UnicodeError);

        DateTimeOffset ISnowflakeEntity.CreatedAt
            => CreatedAt ?? throw new InvalidOperationException(UnicodeError);

        public override string ToString()
            => _emote?.ToString() ?? _emoji?.ToString() ?? "null";

        public static EphemeralEmoji FromRawData(string name, ulong? id = null, bool isAnimated = false)
        {
            var animatedPrefix = isAnimated
                ? "a"
                : string.Empty;

            var raw = $"<{animatedPrefix}:{name}:{id}>";

            if (Emote.TryParse(raw, out var emote))
            {
                return new EphemeralEmoji()
                    .WithEmoteData(emote);
            }
            else
            {
                var emoji = new Discord.Emoji(name);

                return new EphemeralEmoji()
                    .WithEmojiData(emoji);
            }
        }

        public EphemeralEmoji WithEmoteData(Emote emote)
        {
            _emote = emote;
            return this;
        }

        public EphemeralEmoji WithEmojiData(Discord.Emoji emoji)
        {
            _emoji = emoji;
            return this;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as EphemeralEmoji);
        }

        public bool Equals(EphemeralEmoji? other)
        {
            return other != null &&
                   EqualityComparer<ulong?>.Default.Equals(Id, other.Id) &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }

        private const string UnicodeError = "This operation is unavailable for Unicode emoji.";

        private Emote? _emote;
        private Discord.Emoji? _emoji;
    }
}
