using Microsoft.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace BlackFox.Roslyn.TestDiagnostics.RoslynExtensions
{
    public static class DocumentExtensions
    {
        public static async Task<Document> ReplaceNode<TRoot, TNode>(this Document document, TNode oldNode,
            TNode newNode, CancellationToken cancellationToken = default(CancellationToken))
            where TRoot : SyntaxNode
            where TNode : SyntaxNode
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(oldNode, newNode);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
