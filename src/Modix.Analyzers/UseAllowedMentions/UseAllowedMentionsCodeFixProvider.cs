using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Modix.Analyzers.UseAllowedMentions
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class UseAllowedMentionsCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Add allowed mentions";

        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(UseAllowedMentionsAnalyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync();
            var node = root?.FindNode(context.Span);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (node is not InvocationExpressionSyntax invocation)
                    return;

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: Title,
                        createChangedDocument: x => AddAllowedMentionsAsync(context.Document, invocation, x),
                        equivalenceKey: Title),
                    diagnostic);
            }
        }

        private async Task<Document> AddAllowedMentionsAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

            var invocationWithAllowedMentions = invocation.AddArgumentListArguments(
                Argument(
                    NameColon("allowedMentions"),
                    default,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("AllowedMentions"),
                        IdentifierName("None"))));

            editor.ReplaceNode(invocation, invocationWithAllowedMentions);

            return editor.GetChangedDocument();
        }
    }
}
