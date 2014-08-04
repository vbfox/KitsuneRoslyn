using BlackFox.Roslyn.Diagnostics.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.Diagnostics
{
    public abstract class CodeRefactoringProviderBase<TNode> : ICodeRefactoringProvider
        where TNode : SyntaxNode
    {
        private async Task<TNode> GetRefactoringTargetAsync(Document document, TextSpan span,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            if (span.IsEmpty)
            {
                // No selection: The refactoring is called from inside the node
                var child = ChildThatContainsPositionRecursive(root, span.Start);
                return child.FirstAncestorOrSelf<TNode>();
            }
            else
            {
                // A selection exists: The refactoring is called after selecting a range
                return root.DescendantNodes(span).OfType<TNode>().FirstOrDefault();
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

        public async Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TextSpan span,
            CancellationToken cancellationToken)
        {
            var target = await GetRefactoringTargetAsync(document, span, cancellationToken);
            if (target == null)
            {
                return Enumerable.Empty<CodeAction>();
            }

            return await GetRefactoringsAsync(document, target, cancellationToken)
                .ConfigureAwait(false);
        }

        protected abstract Task<IEnumerable<CodeAction>> GetRefactoringsAsync(Document document, TNode node,
            CancellationToken cancellationToken);
    }

}
