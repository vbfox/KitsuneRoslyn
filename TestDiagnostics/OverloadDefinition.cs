using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace BlackFox.Roslyn.TestDiagnostics
{
    class OverloadDefinition
    {
        public ImmutableArray<Func<ITypeSymbol, bool>> Arguments { get; private set; }

        public OverloadDefinition(params Func<ITypeSymbol, bool>[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }

            Arguments = ImmutableArray.Create(arguments);
        }

        public bool IsOverload(IMethodSymbol method)
        {
            var parameters = method.Parameters;

            return Arguments.Length == parameters.Length
                && Arguments.Zip(parameters, (argCheck, p) => argCheck(p.Type)).All(b => b);
        }
    }
}
