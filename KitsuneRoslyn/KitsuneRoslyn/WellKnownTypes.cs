// Copyright (c) Julien Roncaglia.  All Rights Reserved.
// Licensed under the BSD 2-Clause License.
// See LICENSE.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace BlackFox.KitsuneRoslyn
{
    static class WellKnownTypes
    {
        public static INamedTypeSymbol Guid(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Guid");
        }

        public static INamedTypeSymbol Task(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        }
    }
}
