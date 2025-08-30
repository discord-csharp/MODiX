using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Modix.Analyzers.AddDoNotDefer;
using NUnit.Framework;

namespace Modix.Analyzers.Test
{
    using VerifyCodeFix = CSharpCodeFixVerifier<AddDoNotDeferAnalyzer, AddDoNotDeferCodeFixProvider, DefaultVerifier>;

    [TestFixture]
    public class AddDoNotDeferTests
    {
        [Test]
        public async Task SlashCommand_NeedsDoNotDefer_HasDiagnostic()
        {
            var source = @"
using System;
using System.Threading.Tasks;

public class UnitTestModule
{
    {|#0:[SlashCommand(""command"")]
    public async Task CommandAsync()
    {
        await RespondWithModalAsync<IModal>(""test_modal"");
    }|}

    public async Task RespondWithModalAsync<T>(string text) where T : IModal {}
}

public interface IModal {}
public class DoNotDeferAttribute : Attribute {}
public class SlashCommandAttribute : Attribute
{
    public SlashCommandAttribute(string command) {}
}
";

            var fixedSource = @"
using System;
using System.Threading.Tasks;

public class UnitTestModule
{
    [SlashCommand(""command"")]
    [DoNotDefer]
    public async Task CommandAsync()
    {
        await RespondWithModalAsync<IModal>(""test_modal"");
    }

    public async Task RespondWithModalAsync<T>(string text) where T : IModal {}
}

public interface IModal {}
public class DoNotDeferAttribute : Attribute {}
public class SlashCommandAttribute : Attribute
{
    public SlashCommandAttribute(string command) {}
}
";

            var expected = VerifyCodeFix
                .Diagnostic(AddDoNotDeferAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("CommandAsync");

            await VerifyCodeFix.VerifyCodeFixAsync(source, expected, fixedSource);
        }
    }
}
