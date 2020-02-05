using System;
using System.Diagnostics;

namespace Modix.Data.Models.Core
{
    [DebuggerDisplay("({Date}: {MessageCount})")]
    public class MessageCountByDate
    {
        public DateTime Date { get; set; }
        public int MessageCount { get; set; }
    }
}
