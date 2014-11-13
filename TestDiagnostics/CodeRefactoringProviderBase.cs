using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace BlackFox.Roslyn.Diagnostics
{
    public abstract class CodeRefactoringProviderBase<TNode> : CodeRefactoringProvider
        where TNode : SyntaxNode
    {
        private async Task<TNode> GetRefactoringTargetAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            if (context.Span.IsEmpty)
            {
                // No selection: The refactoring is called from inside the node
                var child = ChildThatContainsPositionRecursive(root, context.Span.Start);
                return child.FirstAncestorOrSelf<TNode>();
            }
            else
            {
                // A selection exists: The refactoring is called after selecting a range
                return root.DescendantNodes(context.Span).OfType<TNode>().FirstOrDefault();
            }
        }

        private SyntaxNodeOrToken ChildThatContainsPositionRecursive(SyntaxNode node, int position)
        {
            if (node.ChildNodesAndTokens().Any())
            {
                var containsPosition = node.ChildThatContainsPosition(position);
                if (containsPosition.IsNode)
                {
                    return ChildThatContainsPositionRecursive(containsPosition.AsNode(), position);
                }
                else
                {
                    return containsPosition;
                }
            }
            else
            {
                return node;
            }
        }

        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var target = await GetRefactoringTargetAsync(context);
            if (target == null)
            {
                return;
            }

            await GetRefactoringsAsync(context, target).ConfigureAwait(false);
        }

        protected abstract Task GetRefactoringsAsync(CodeRefactoringContext context, TNode node);
    }

}
