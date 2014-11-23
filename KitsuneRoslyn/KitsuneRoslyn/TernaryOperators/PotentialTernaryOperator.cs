// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Linq;

namespace BlackFox.Roslyn.Diagnostics.TernaryOperators
{
    class PotentialTernaryOperator
    {
        public static PotentialTernaryOperator NoReplacement { get; }
            = new PotentialTernaryOperator(
                    PotentialTernaryOperatorClassification.NoReplacement,
                    ImmutableList<StatementSyntax>.Empty,
                    0,
                    ImmutableList<SyntaxNode>.Empty);

        public PotentialTernaryOperatorClassification Classification { get; private set; }
        public ImmutableList<SyntaxNode> NodesToRemove { get; private set; }
        public ImmutableList<StatementSyntax> Replacements { get; private set; }
        public int TernaryOperatorCount { get; private set; }

        PotentialTernaryOperator(PotentialTernaryOperatorClassification classification,
            ImmutableList<StatementSyntax> replacements, int ternaryOperatorCount, ImmutableList<SyntaxNode> toRemove)
        {
            Parameter.MustNotBeNull(replacements, nameof(replacements));
            Parameter.MustNotBeNull(toRemove, nameof(toRemove));

            Classification = classification;
            NodesToRemove = toRemove;
            Replacements = replacements;
            TernaryOperatorCount = ternaryOperatorCount;
        }


        static ExpressionSyntax ParenthesesAroundConditionalExpression(ExpressionSyntax expression)
        {
            var conditional = expression as ConditionalExpressionSyntax;
            return conditional != null
                ? conditional.WithParentheses()
                : expression;
        }

        public static ConditionalExpressionSyntax BuildReplacement(ExpressionSyntax condition, ExpressionSyntax whenTrue,
            ExpressionSyntax whenFalse)
        {
            var conditional = ConditionalExpression(
                condition,
                ParenthesesAroundConditionalExpression(whenTrue),
                ParenthesesAroundConditionalExpression(whenFalse));

            return conditional
                .WithAdditionalAnnotations(SyntaxAnnotation.ElasticAnnotation, Formatter.Annotation);
        }

        public static PotentialTernaryOperator Create(IfStatementSyntax ifStatement)
        {
            ImmutableList<SyntaxNode> virtualElseStatements;
            var elseStatement = GetRealOrVirtualElse(ifStatement, out virtualElseStatements);
            if (!elseStatement.HasValue)
            {
                return NoReplacement;
            }

            var replaceable = TernaryReplaceable.Find(ifStatement.Statement, elseStatement.Value);

            if (!replaceable.IsReplaceable)
            {
                return NoReplacement;
            }

            var toTrack = replaceable.Differences.Select(t => t.Item1);
            var replacement = ifStatement.Statement.TrackNodes(toTrack);

            foreach (var difference in replaceable.Differences)
            {
                var conditionalReplacement = BuildReplacement(ifStatement.Condition, difference.Item1, difference.Item2);
                replacement = replacement.ReplaceNode(replacement.GetCurrentNode(difference.Item1), conditionalReplacement);
            }

            return new PotentialTernaryOperator(PotentialTernaryOperatorClassification.ReplacementPossible,
                BlockContent(replacement), replaceable.Differences.Count, virtualElseStatements);
        }

        private static Optional<StatementSyntax> GetRealOrVirtualElse(IfStatementSyntax ifStatement,
            out ImmutableList<SyntaxNode> virtualElseStatements)
        {
            if (ifStatement.Else != null)
            {
                virtualElseStatements = ImmutableList<SyntaxNode>.Empty;
                return ifStatement.Else.Statement;
            }

            var ifBlock = ifStatement.Statement as BlockSyntax;
            var statementCount = ifBlock != null ? ifBlock.Statements.Count : 1;
            var sibilings = ifStatement.Parent.ChildNodes();
            var potential = sibilings.SkipWhile(n => n != ifStatement).Skip(1).Take(statementCount).ToImmutableList();

            if (potential.Count != statementCount)
            {
                virtualElseStatements = ImmutableList<SyntaxNode>.Empty;
                return null;
            }

            virtualElseStatements = potential;
            return Block(List(potential));
        }

        static ImmutableList<StatementSyntax> BlockContent(StatementSyntax node)
        {
            var block = node as BlockSyntax;
            if (block != null)
            {
                return block.Statements.ToImmutableList();
            }
            else
            {
                return ImmutableList<StatementSyntax>.Empty.Add(node);
            }
        } 
    }
}
