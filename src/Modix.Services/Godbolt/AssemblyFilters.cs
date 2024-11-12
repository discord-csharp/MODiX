#nullable enable
namespace Modix.Services.Godbolt
{
    internal record class AssemblyFilters
    {
        public bool Binary { get; set; } = false;
        public bool CommentOnly { get; set; } = true;
        public bool Directives { get; set; } = true;
        public bool Labels { get; set; } = true;
        public bool Trim { get; set; } = true;
        public bool Demangle { get; set; } = true;
        public bool Intel { get; set; } = true;
        public bool Execute { get; set; } = false;
    }
}
