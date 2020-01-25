using System;
using System.Collections.Generic;
using Modix.Data.Models.Moderation;

namespace Modix.Services.UserInfo
{
    internal class UserInfoUserDto
    {
        public DateTimeOffset FirstSeen { get; set; }
        public DateTimeOffset LastSeen { get; set; }
        public List<UserInfoInfractionDto> Infractions { get; set; } = null!;
        public List<UserInfoPromotionCampaignDto> Promotions { get; set; } = null!;

        public class UserInfoInfractionDto
        {
            public InfractionType Type { get; set; }
            public DateTimeOffset Created { get; set; }
            public string Reason { get; set; } = null!;
        }

        public class UserInfoPromotionCampaignDto
        {
            public DateTimeOffset ClosedDate { get; set; }
            public ulong TargetRoleId { get; set; }
        }
    }
}
