using Modix.Data.Models.Core;

namespace Modix.Web.Models.Configuration;

public record DesignatedChannelData(long Id, ulong RoleId, DesignatedChannelType ChannelDesignation, string Name);
