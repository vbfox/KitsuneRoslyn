// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;

namespace BlackFox.Roslyn.Diagnostics.TernaryOperators
{
    public class TernaryReplaceableResult
    {
        public bool IsReplaceable { get; }
        public ImmutableList<Tuple<ExpressionSyntax, ExpressionSyntax>> Differences { get; }

        public TernaryReplaceableResult(bool success, ImmutableList<Tuple<ExpressionSyntax, ExpressionSyntax>> differences)
        {
            Parameter.MustNotBeNull(differences, nameof(differences));

            IsReplaceable = success;
            Differences = differences;
        }
    }
}
