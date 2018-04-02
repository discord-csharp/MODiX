using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Services.CommandHelp
{
    /// <summary>
    /// Hides the module or command from display
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HiddenFromHelpAttribute : Attribute
    {

    }
}
