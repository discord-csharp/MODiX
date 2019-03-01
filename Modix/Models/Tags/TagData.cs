using System;

namespace Modix.Models.Tags
{
    public class TagData
    {
        public string Name { get; set; }

        public DateTimeOffset Created { get; set; }

        public bool IsOwnedByRole { get; set; }

        public string OwnerName { get; set; }

        public string OwnerColor { get; set; }

        public string Content { get; set; }

        public uint Uses { get; set; }

        public bool CanMaintain { get; set; }
    }
}
