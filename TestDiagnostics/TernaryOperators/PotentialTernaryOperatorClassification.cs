// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

namespace BlackFox.Roslyn.Diagnostics.TernaryOperators
{
    enum PotentialTernaryOperatorClassification
    {
        NoReplacement,
        Return,
        Assignment
    }
}
