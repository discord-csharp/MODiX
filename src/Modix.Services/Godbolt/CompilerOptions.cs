#nullable enable
namespace Modix.Services.Godbolt
{
    internal record class CompilerOptions
    {
        public string UserArguments { get; set; } = "";
        public AssemblyFilters Filters { get; } = new();
    }
}
