// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.RoslynExtensions
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
                .Where(x => x.GetLocation().SourceSpan == diagnosticSpan)
                .OfType<TSyntaxNode>()
                .First();
        }
    }
}
