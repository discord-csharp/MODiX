using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Modix.Analyzers.UseAllowedMentions;
using NUnit.Framework;

namespace Modix.Analyzers.Test
{
    using VerifyCodeFix = CSharpCodeFixVerifier<UseAllowedMentionsAnalyzer, UseAllowedMentionsCodeFixProvider, DefaultVerifier>;

    [TestFixture]
    public class UseAllowedMentionsTests
    {
        [Test]
        public async Task FollowupAsync_NeedsAllowedMentions_HasDiagnostic()
        {
            var source = @"
using System;
using System.Threading.Tasks;

public class UnitTestModule
{
    [SlashCommand(""command"")]
    public async Task CommandAsync()
    {
        await {|#0:FollowupAsync(""@everyone"")|};
    }

    public async Task FollowupAsync(string text, AllowedMentions allowedMentions = null) {}
}

public class SlashCommandAttribute : Attribute
{
    public SlashCommandAttribute(string command) {}
}

public class AllowedMentions
{
    public static AllowedMentions None => new();
}
";

            var fixedSource = @"
using System;
using System.Threading.Tasks;

public class UnitTestModule
{
    [SlashCommand(""command"")]
    public async Task CommandAsync()
    {
        await FollowupAsync(""@everyone"", allowedMentions: AllowedMentions.None);
    }

    public async Task FollowupAsync(string text, AllowedMentions allowedMentions = null) {}
}

public class SlashCommandAttribute : Attribute
{
    public SlashCommandAttribute(string command) {}
}

public class AllowedMentions
{
    public static AllowedMentions None => new();
}
";

            var expected = VerifyCodeFix
                .Diagnostic(UseAllowedMentionsAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("FollowupAsync");

            await VerifyCodeFix.VerifyCodeFixAsync(source, expected, fixedSource);
        }
    }
}
