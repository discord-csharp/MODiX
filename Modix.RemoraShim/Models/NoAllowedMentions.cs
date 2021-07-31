using System;

using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Modix.RemoraShim.Models
{
    /// <summary>
    /// Represents an AllowedMentions object that disallows all mentions.
    /// </summary>
    internal record NoAllowedMentions() : AllowedMentions(Parse: Array.Empty<MentionType>());
}
