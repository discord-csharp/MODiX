using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Modix.Analyzers.AddDoNotDefer
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class AddDoNotDeferCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Add DoNotDeferAttribute";

        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(AddDoNotDeferAnalyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync();
            var node = root?.FindNode(context.Span);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (node is not MethodDeclarationSyntax method)
                    return;

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: Title,
                        createChangedDocument: x => AddDoNotDeferAttributeAsync(context.Document, method, x),
                        equivalenceKey: Title),
                    diagnostic);
            }
        }

        private async Task<Document> AddDoNotDeferAttributeAsync(Document document, MethodDeclarationSyntax method, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

            var methodWithDoNotDefer = method.AddAttributeLists(
                AttributeList(SingletonSeparatedList(
                    Attribute(IdentifierName("DoNotDefer")))));

            editor.ReplaceNode(method, methodWithDoNotDefer);

            return editor.GetChangedDocument();
        }
    }
}
