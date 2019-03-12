using System;
using System.Collections.Generic;
using Discord;

namespace Modix.Data.Models.Reactions
{
    public sealed class EphemeralEmoji : ISnowflakeEntity, IEmote
    {
        public ulong? Id
            => _emote?.Id;

        public DateTimeOffset? CreatedAt
            => _emote?.CreatedAt;

        public string Name
            => _emote?.Name ?? _emoji?.Name;

        public bool Animated
            => _emote?.Animated ?? false;

        public string Url
            => _emote?.Url;

        ulong IEntity<ulong>.Id
            => Id ?? throw new InvalidOperationException(UnicodeError);

        DateTimeOffset ISnowflakeEntity.CreatedAt
            => CreatedAt ?? throw new InvalidOperationException(UnicodeError);

        public override string ToString()
            => _emote?.ToString() ?? _emoji?.ToString();

        public static EphemeralEmoji FromRawData(string name, ulong? id = null)
        {
            var raw = $"<:{name}:{id}>";

            if (Emote.TryParse(raw, out var emote))
            {
                return new EphemeralEmoji()
                    .WithEmoteData(emote);
            }
            else
            {
                var emoji = new Emoji(name);

                return new EphemeralEmoji()
                    .WithEmojiData(emoji);
            }
        }

        public EphemeralEmoji WithEmoteData(Emote emote)
        {
            _emote = emote;
            return this;
        }

        public EphemeralEmoji WithEmojiData(Emoji emoji)
        {
            _emoji = emoji;
            return this;
        }

        private const string UnicodeError = "This operation is unavailable for Unicode emoji.";

        private Emote _emote;
        private Emoji _emoji;

        public class EqualityComparer : IEqualityComparer<EphemeralEmoji>
        {
            public bool Equals(EphemeralEmoji x, EphemeralEmoji y)
                => x.Id == y.Id
                && x.Name == y.Name;

            public int GetHashCode(EphemeralEmoji obj)
            {
                var hashCode = new HashCode();

                hashCode.Add(obj.Name);

                if (obj.Id != null)
                    hashCode.Add(obj.Id);

                return hashCode.ToHashCode();
            }
        }
    }
}
