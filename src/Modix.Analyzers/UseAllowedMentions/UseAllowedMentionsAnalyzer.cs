using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Modix.Analyzers.UseAllowedMentions
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseAllowedMentionsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MDX001";

        private const string Title = "Allowed mentions should be specified";
        private const string MessageFormat = "Invocation of method '{0}' should specify allowed mentions";
        private const string Description = "When calling methods that send a message to Discord, specify allowed mentions to avoid unintentionally mentioning people.";
        private const string Category = "Discord";

        private static readonly DiagnosticDescriptor _descriptor = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
        private static readonly ImmutableArray<OperationKind> _supportedOperations = ImmutableArray.Create(OperationKind.Invocation);
        private static readonly ImmutableArray<string> _methodNames = ImmutableArray.Create("FollowupAsync", "ReplyAsync", "RespondAsync", "SendMessageAsync");

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
            var invocation = (IInvocationOperation)context.Operation;
            var methodName = invocation.TargetMethod.Name;

            if (!_methodNames.Contains(methodName))
                return;

            var textArgument = invocation.Arguments.FirstOrDefault(ArgumentIsForTextParameter);

            if (textArgument is null || textArgument.IsImplicit || ArgumentValueIsNullLiteral(textArgument) || ArgumentIsLiteralWithoutEveryoneOrHere(textArgument))
                return;

            var allowedMentionsArgument = invocation.Arguments.FirstOrDefault(ArgumentIsForAllowedMentionsParameter);

            if (allowedMentionsArgument is null || !allowedMentionsArgument.IsImplicit && !ArgumentValueIsNullLiteral(allowedMentionsArgument))
                return;

            var diagnostic = Diagnostic.Create(_descriptor, invocation.Syntax.GetLocation(), methodName);
            context.ReportDiagnostic(diagnostic);

            static bool ArgumentIsForTextParameter(IArgumentOperation x)
                => x.Parameter is { Name: "text" or "message" };

            static bool ArgumentValueIsNullLiteral(IArgumentOperation argument)
                => argument.Value is ILiteralOperation { ConstantValue: { HasValue: true, Value: null } };

            static bool ArgumentIsLiteralWithoutEveryoneOrHere(IArgumentOperation argument)
                => argument.Value is ILiteralOperation { ConstantValue: { HasValue: true, Value: string stringValue } }
                && (string.IsNullOrWhiteSpace(stringValue) || !stringValue.Contains("@everyone") && !stringValue.Contains("@here"));

            static bool ArgumentIsForAllowedMentionsParameter(IArgumentOperation x)
                => x.Parameter?.Name == "allowedMentions";
        }
    }
}
