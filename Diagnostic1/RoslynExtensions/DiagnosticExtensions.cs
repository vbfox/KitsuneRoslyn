using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace BlackFox.Roslyn.TestDiagnostics.RoslynExtensions
{
    static class DiagnosticExtensions
    {
        public static TSyntaxNode GetAncestorSyntaxNode<TSyntaxNode>(this Diagnostic diagnostic, SyntaxNode root)
            where TSyntaxNode : SyntaxNode
        {
            if (diagnostic == null)
            {
                throw new ArgumentNullException("diagnostic");
            }
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

            var diagnosticSpan = diagnostic.Location.SourceSpan;

            return root.FindToken(diagnosticSpan.Start)
                .Parent
                .AncestorsAndSelf()
                .OfType<TSyntaxNode>()
                .First();
        }
    }
}
