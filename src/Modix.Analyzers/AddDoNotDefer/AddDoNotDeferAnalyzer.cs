using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Modix.Analyzers.AddDoNotDefer
{
    public class AddDoNotDeferAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MDX002";

        private const string Title = "Add DoNotDeferAttribute";
        private const string MessageFormat = "Command '{0}' should have DoNotDeferAttribute";
        private const string Description = "DoNotDeferAttribute should be added to commands that respond rather than follow-up to an interaction.";
        private const string Category = "Discord";

        private static readonly DiagnosticDescriptor _descriptor = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
        private static readonly ImmutableArray<OperationKind> _supportedOperations = ImmutableArray.Create(OperationKind.MethodBody);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(_descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterOperationAction(Analyze, _supportedOperations);
        }

        private static void Analyze(OperationAnalysisContext context)
        {
            var method = (IMethodBodyOperation)context.Operation;

            if (method.Syntax is not MethodDeclarationSyntax methodSyntax)
                return;

            var attributes = methodSyntax.AttributeLists.SelectMany(x => x.Attributes);

            if (!AttributesContains(attributes, "SlashCommand"))
                return;

            if (AttributesContains(attributes, "DoNotDefer"))
                return;

            if (!MethodContainsResponseMethodInvocation(method))
                return;

            var diagnostic = Diagnostic.Create(_descriptor, method.Syntax.GetLocation(), methodSyntax.Identifier.Text);
            context.ReportDiagnostic(diagnostic);

            static bool AttributesContains(IEnumerable<AttributeSyntax> attributes, string attributeName)
                => attributes.Any(x => x.Name.ToString() is var name && (name == attributeName || name == $"{attributeName}Attribute"));

            static bool MethodContainsResponseMethodInvocation(IMethodBodyOperation method)
                => (method.BlockBody ?? method.ExpressionBody)!
                    .Operations
                    .SelectMany(x => x.DescendantsAndSelf())
                    .OfType<IInvocationOperation>()
                    .Any(InvocationIsResponseMethod);

            static bool InvocationIsResponseMethod(IInvocationOperation x)
                => x.TargetMethod.Name is var name && name.StartsWith("Respond") && name.EndsWith("Async");
        }
    }
}
