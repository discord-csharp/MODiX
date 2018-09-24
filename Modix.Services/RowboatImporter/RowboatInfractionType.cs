using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Modix.Services.RowboatImporter
{
    public enum RowboatInfractionType
    {
        Mute = 0, 
        Kick = 1, 
        Tempban = 2, 
        Ban = 4, 
        Tempmute = 5, 
        Unban = 6, 
        Warning = 8 
    }
}
