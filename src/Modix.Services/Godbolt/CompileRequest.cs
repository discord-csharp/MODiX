#nullable enable
namespace Modix.Services.Godbolt
{
    internal record class CompileRequest(string Lang, string Source)
    {
        public string Compiler { get; } = $"dotnettrunk{Lang}coreclr";
        public CompilerOptions Options { get; } = new();
    }
}
