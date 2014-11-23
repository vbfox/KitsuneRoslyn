using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlackFox.KitsuneRoslyn.UnusedLocalVariable
{
    class PotentialSideEffectAnalysis
    {
        private class State
        {
            public bool CreateInstances { get; set; }
            public bool CallMethods { get; set; }
            public bool ReadPropertyValues { get; set; }
            public bool WriteValues { get; set; }
            public bool InvokeEvents { get; set; }
        }

        private readonly State state;

        public bool CreateInstances { get { return state.CreateInstances; } }
        public bool CallMethods { get { return state.CallMethods; } }
        public bool ReadPropertyValues { get { return state.ReadPropertyValues; } }
        public bool WriteValues { get { return state.WriteValues; } }

        private PotentialSideEffectAnalysis(State state)
        {
            this.state = state;
        }

        private class PotentialSideEffectVisitor : CSharpSyntaxWalker
        {
            SemanticModel semanticModel;
            public State State { get; } = new State();

            public PotentialSideEffectVisitor(SemanticModel semanticModel)
            {
                this.semanticModel = semanticModel;
            }

            public override void DefaultVisit(SyntaxNode node)
            {
                if (node is AnonymousMethodExpressionSyntax || node is SimpleLambdaExpressionSyntax)
                {
                    // Don't descend into them we don't care
                    return;
                }

                base.DefaultVisit(node);
            }

            public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
            {
                State.CreateInstances = true;

                base.VisitObjectCreationExpression(node);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                // TODO: Distinguish between [Pure] methods and others
                State.CallMethods=true;

                base.VisitInvocationExpression(node);
            }

            public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
            {
                State.WriteValues = true;
                base.VisitAssignmentExpression(node);
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                var symbol = semanticModel.GetSymbolInfo(node).Symbol;
                if (symbol != null && symbol.Kind == SymbolKind.Property)
                {
                    State.ReadPropertyValues = true;
                }
                base.VisitMemberAccessExpression(node);
            }
        }

        /// <summary>
        /// Use heuristics to determine if an expression can have side effects.
        /// </summary>
        public static PotentialSideEffectAnalysis Analyze(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var visitor = new PotentialSideEffectVisitor(semanticModel);
            expression.Accept(visitor);
            return new PotentialSideEffectAnalysis(visitor.State);
        }
    }
}
