using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace BlackFox.Roslyn.Diagnostics.MethodCanBeMadeStatic
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodCanBeMadeStaticAnalyzer : DiagnosticAnalyzer
    {
        public const string Id = "BlackFox.MethodCanBeMadeStatic";

        public static DiagnosticDescriptor Descriptor { get; }
            = new DiagnosticDescriptor(
                Id,
                "Method can be made static",
                "Method can be made static",
                "Readability",
                DiagnosticSeverity.Info,
                isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
            = ImmutableArray.Create(SyntaxKind.MethodDeclaration);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            /*
            lock (methodDeclarations)
            {
                methodDeclarations = methodDeclarations.Add((MethodDeclarationSyntax)node);
            }*/
            
            var method = (MethodDeclarationSyntax)context.Node;

            var analysis = context.SemanticModel.AnalyzeIfMethodCanBeMadeStatic(method, currentCompilation, context.CancellationToken);

            if (analysis.CanBeMadeStatic)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, method.Identifier.GetLocation()));
            }
        }

        /*
        ImmutableList<MethodDeclarationSyntax> methodDeclarations = ImmutableList<MethodDeclarationSyntax>.Empty;

        public void OnCompilationEnded(Compilation compilation, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            lock (methodDeclarations)
            {
                foreach (var method in methodDeclarations)
                {
                    var semanticModel = compilation.GetSemanticModel(method.SyntaxTree);
                    if (semanticModel.CanBeMadeStatic(method, compilation, cancellationToken))
                    {
                        addDiagnostic(Diagnostic.Create(Descriptor, method.Identifier.GetLocation()));
                    }
                }
            }
        }
        */
        public void CompilationStart(CompilationStartAnalysisContext context)
        {/*
            lock(methodDeclarations)
            {
                methodDeclarations = ImmutableList<MethodDeclarationSyntax>.Empty;
            }
            return this;*/
            currentCompilation = context.Compilation;
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(CompilationStart);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        Compilation currentCompilation;
    }


}
