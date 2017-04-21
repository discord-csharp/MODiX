using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Services.Csharp
{
    public class DocumentationApiResponse
    {
        public List<DocumentationMember> Results { get; set; }= new List<DocumentationMember>();
        public int Count { get; set; }
    }

    public class DocumentationMember
    {
        public string DisplayName { get; set; }
        public string Url { get; set; }
        public Type ItemType { get; set; }
        public Kind ItemKind { get; set; }
        public string Description { get; set; }
    }

    public enum Type
    {
        Type,
        Namespace,
        Member,
    }

    public enum Kind
    {
        Namespace,
        Class,
        Enumeration,
        Method,
        Structure,
        Property,
        Constructor,
        Field,
        Event,
        Interface,
        Delegate
    }
}
