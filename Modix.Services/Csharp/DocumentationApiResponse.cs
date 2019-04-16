using System.Collections.Generic;

namespace Modix.Services.Csharp
{
    public class DocumentationApiResponse
    {
        public List<DocumentationMember> Results { get; set; } = new List<DocumentationMember>();

        public int Count { get; set; }
    }

    public class DocumentationMember
    {
        public string DisplayName { get; set; }

        public string Url { get; set; }

        public string ItemType { get; set; }

        public string ItemKind { get; set; }

        public string Description { get; set; }
    }
}
