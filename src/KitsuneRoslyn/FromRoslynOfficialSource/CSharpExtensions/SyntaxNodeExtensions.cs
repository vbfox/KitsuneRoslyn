// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Extensions
{
    internal static partial class SyntaxNodeExtensions
    {
        public static T WithPrependedLeadingTrivia<T>(
            this T node,
            params SyntaxTrivia[] trivia) where T : SyntaxNode
        {
            if (trivia.Length == 0)
            {
                return node;
            }

            return node.WithPrependedLeadingTrivia((IEnumerable<SyntaxTrivia>)trivia);
        }

        public static T WithPrependedLeadingTrivia<T>(
            this T node,
            SyntaxTriviaList trivia) where T : SyntaxNode
        {
            if (trivia.Count == 0)
            {
                return node;
            }

            return node.WithLeadingTrivia(trivia.Concat(node.GetLeadingTrivia()));
        }

        public static T WithPrependedLeadingTrivia<T>(
            this T node,
            IEnumerable<SyntaxTrivia> trivia) where T : SyntaxNode
        {
            return node.WithPrependedLeadingTrivia(trivia.ToSyntaxTriviaList());
        }

        public static T WithAppendedTrailingTrivia<T>(
            this T node,
            params SyntaxTrivia[] trivia) where T : SyntaxNode
        {
            if (trivia.Length == 0)
            {
                return node;
            }

            return node.WithAppendedTrailingTrivia((IEnumerable<SyntaxTrivia>)trivia);
        }

        public static T WithAppendedTrailingTrivia<T>(
            this T node,
            SyntaxTriviaList trivia) where T : SyntaxNode
        {
            if (trivia.Count == 0)
            {
                return node;
            }

            return node.WithTrailingTrivia(node.GetTrailingTrivia().Concat(trivia));
        }

        public static T WithAppendedTrailingTrivia<T>(
            this T node,
            IEnumerable<SyntaxTrivia> trivia) where T : SyntaxNode
        {
            return node.WithAppendedTrailingTrivia(trivia.ToSyntaxTriviaList());
        }

        public static T With<T>(
            this T node,
            IEnumerable<SyntaxTrivia> leadingTrivia,
            IEnumerable<SyntaxTrivia> trailingTrivia) where T : SyntaxNode
        {
            return node.WithLeadingTrivia(leadingTrivia).WithTrailingTrivia(trailingTrivia);
        }
    }
}