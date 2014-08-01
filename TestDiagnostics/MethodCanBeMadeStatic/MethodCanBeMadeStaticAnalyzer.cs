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
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("BlackFox.MethodCanBeMadeStaticAnalyzer", LanguageNames.CSharp)]
    public class MethodCanBeMadeStaticAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>,
        ICompilationStartedAnalyzer/*, ICompilationEndedAnalyzer*/
    {
        public const string Id = "BlackFox.MethodCanBeMadeStatic";

        public static DiagnosticDescriptor Descriptor { get; }
            = new DiagnosticDescriptor(
                Id,
                "Method can be made static",
                "Method can be made static",
                "Readability",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get; }
            = ImmutableArray.Create(SyntaxKind.MethodDeclaration);

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic,
            CancellationToken cancellationToken)
        {
            /*
            lock (methodDeclarations)
            {
                methodDeclarations = methodDeclarations.Add((MethodDeclarationSyntax)node);
            }*/
            
            var method = (MethodDeclarationSyntax)node;

            if (semanticModel.CanBeMadeStatic(method, currentCompilation, cancellationToken))
            {
                addDiagnostic(Diagnostic.Create(Descriptor, method.Identifier.GetLocation()));
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
        public ICompilationEndedAnalyzer OnCompilationStarted(Compilation compilation,
            Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        {/*
            lock(methodDeclarations)
            {
                methodDeclarations = ImmutableList<MethodDeclarationSyntax>.Empty;
            }
            return this;*/
            currentCompilation = compilation;
            return null;
        }

        Compilation currentCompilation;
    }


}
