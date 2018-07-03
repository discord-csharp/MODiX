using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Services.Authorization
{
    public enum AuthorizationClaims
    {
        ModerationRead,
        ModerationNote,
        ModerationWarn,
        ModerationMute,
        ModerationBan,
        ModerationRescind
    }
}
