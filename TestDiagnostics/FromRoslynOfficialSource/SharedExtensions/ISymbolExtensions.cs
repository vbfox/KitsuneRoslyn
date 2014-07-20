// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Roslyn.Utilities;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.CodeAnalysis.Shared.Extensions
{
    internal static partial class ISymbolExtensions
    {
        public static ISymbol OverriddenMember(this ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Event:
                    return ((IEventSymbol)symbol).OverriddenEvent;

                case SymbolKind.Method:
                    return ((IMethodSymbol)symbol).OverriddenMethod;

                case SymbolKind.Property:
                    return ((IPropertySymbol)symbol).OverriddenProperty;
            }

            return null;
        }

        public static bool IsOverridable(this ISymbol symbol)
        {
            return
                symbol != null &&
                symbol.ContainingType != null &&
                symbol.ContainingType.TypeKind == TypeKind.Class &&
                (symbol.IsVirtual || symbol.IsAbstract || symbol.IsOverride) &&
                !symbol.IsSealed;
        }

        public static bool IsImplementable(this ISymbol symbol)
        {
            if (symbol != null &&
                symbol.ContainingType != null &&
                symbol.ContainingType.TypeKind == TypeKind.Interface)
            {
                if (symbol.Kind == SymbolKind.Event)
                {
                    return true;
                }

                if (symbol.Kind == SymbolKind.Property)
                {
                    return true;
                }

                if (symbol.Kind == SymbolKind.Method && ((IMethodSymbol)symbol).MethodKind == MethodKind.Ordinary)
                {
                    return true;
                }
            }

            return false;
        }

        public static INamedTypeSymbol GetContainingTypeOrThis(this ISymbol symbol)
        {
            if (symbol is INamedTypeSymbol)
            {
                return (INamedTypeSymbol)symbol;
            }

            return symbol.ContainingType;
        }

        public static bool IsPointerType(this ISymbol symbol)
        {
            return symbol is IPointerTypeSymbol;
        }

        public static bool IsErrorType(this ISymbol symbol)
        {
            return
                symbol is ITypeSymbol &&
                ((ITypeSymbol)symbol).TypeKind == TypeKind.Error;
        }

        public static bool IsModuleType(this ISymbol symbol)
        {
            return
                symbol is ITypeSymbol &&
                ((ITypeSymbol)symbol).TypeKind == TypeKind.Module;
        }

        public static bool IsArrayType(this ISymbol symbol)
        {
            return
                symbol != null &&
                symbol.Kind == SymbolKind.ArrayType;
        }

        public static bool IsAnonymousFunction(this ISymbol symbol)
        {
            return symbol is IMethodSymbol && ((IMethodSymbol)symbol).MethodKind == MethodKind.AnonymousFunction;
        }

        public static bool IsKind(this ISymbol symbol, SymbolKind kind)
        {
            return symbol.MatchesKind(kind);
        }

        public static bool MatchesKind(this ISymbol symbol, SymbolKind kind)
        {
            return symbol != null
                && symbol.Kind == kind;
        }

        public static bool MatchesKind(this ISymbol symbol, SymbolKind kind1, SymbolKind kind2)
        {
            return symbol != null
                && (symbol.Kind == kind1 || symbol.Kind == kind2);
        }

        public static bool MatchesKind(this ISymbol symbol, SymbolKind kind1, SymbolKind kind2, SymbolKind kind3)
        {
            return symbol != null
                && (symbol.Kind == kind1 || symbol.Kind == kind2 || symbol.Kind == kind3);
        }

        public static bool MatchesKind(this ISymbol symbol, params SymbolKind[] kinds)
        {
            return symbol != null
                && kinds.Contains(symbol.Kind);
        }

        public static bool IsReducedExtension(this ISymbol symbol)
        {
            return symbol is IMethodSymbol && ((IMethodSymbol)symbol).MethodKind == MethodKind.ReducedExtension;
        }

        public static bool IsExtensionMethod(this ISymbol symbol)
        {
            return symbol.Kind == SymbolKind.Method && ((IMethodSymbol)symbol).IsExtensionMethod;
        }

        public static bool IsModuleMember(this ISymbol symbol)
        {
            return symbol != null && symbol.ContainingSymbol is INamedTypeSymbol && symbol.ContainingType.TypeKind == TypeKind.Module;
        }

        public static bool IsConstructor(this ISymbol symbol)
        {
            return symbol is IMethodSymbol && ((IMethodSymbol)symbol).MethodKind == MethodKind.Constructor;
        }

        public static bool IsStaticConstructor(this ISymbol symbol)
        {
            return symbol is IMethodSymbol && ((IMethodSymbol)symbol).MethodKind == MethodKind.StaticConstructor;
        }

        public static bool IsDestructor(this ISymbol symbol)
        {
            return symbol is IMethodSymbol && ((IMethodSymbol)symbol).MethodKind == MethodKind.Destructor;
        }

        public static bool IsUserDefinedOperator(this ISymbol symbol)
        {
            return symbol is IMethodSymbol && ((IMethodSymbol)symbol).MethodKind == MethodKind.UserDefinedOperator;
        }

        public static bool IsConversion(this ISymbol symbol)
        {
            return symbol is IMethodSymbol && ((IMethodSymbol)symbol).MethodKind == MethodKind.Conversion;
        }

        public static bool IsOrdinaryMethod(this ISymbol symbol)
        {
            return symbol is IMethodSymbol && ((IMethodSymbol)symbol).MethodKind == MethodKind.Ordinary;
        }

        public static bool IsDelegateType(this ISymbol symbol)
        {
            return symbol is ITypeSymbol && ((ITypeSymbol)symbol).TypeKind == TypeKind.Delegate;
        }

        public static bool IsAnonymousType(this ISymbol symbol)
        {
            return symbol is INamedTypeSymbol && ((INamedTypeSymbol)symbol).IsAnonymousType;
        }

        public static bool IsNormalAnonymousType(this ISymbol symbol)
        {
            return symbol.IsAnonymousType() && !symbol.IsDelegateType();
        }

        public static bool IsAnonymousDelegateType(this ISymbol symbol)
        {
            return symbol.IsAnonymousType() && symbol.IsDelegateType();
        }

        public static bool IsAnonymousTypeProperty(this ISymbol symbol)
        {
            return symbol is IPropertySymbol && symbol.ContainingType.IsNormalAnonymousType();
        }

        public static bool IsIndexer(this ISymbol symbol)
        {
            return symbol is IPropertySymbol && ((IPropertySymbol)symbol).IsIndexer;
        }

        public static bool IsWriteableFieldOrProperty(this ISymbol symbol)
        {
            var fieldSymbol = symbol as IFieldSymbol;
            if (fieldSymbol != null)
            {
                return !fieldSymbol.IsReadOnly
                    && !fieldSymbol.IsConst;
            }

            var propertySymbol = symbol as IPropertySymbol;
            if (propertySymbol != null)
            {
                return !propertySymbol.IsReadOnly;
            }

            return false;
        }

        public static ITypeSymbol GetMemberType(this ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Field:
                    return ((IFieldSymbol)symbol).Type;
                case SymbolKind.Property:
                    return ((IPropertySymbol)symbol).Type;
                case SymbolKind.Method:
                    return ((IMethodSymbol)symbol).ReturnType;
                case SymbolKind.Event:
                    return ((IEventSymbol)symbol).Type;
            }

            return null;
        }

        public static int GetArity(this ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.NamedType:
                    return ((INamedTypeSymbol)symbol).Arity;
                case SymbolKind.Method:
                    return ((IMethodSymbol)symbol).Arity;
                default:
                    return 0;
            }
        }

        public static ISymbol GetOriginalUnreducedDefinition(this ISymbol symbol)
        {
            if (symbol.IsReducedExtension())
            {
                // note: ReducedFrom is only a method definition and includes no type arguments.
                symbol = ((IMethodSymbol)symbol).GetConstructedReducedFrom();
            }

            if (symbol.IsFunctionValue())
            {
                var method = symbol.ContainingSymbol as IMethodSymbol;
                if (method != null)
                {
                    symbol = method;

                    if (method.AssociatedSymbol != null)
                    {
                        symbol = method.AssociatedSymbol;
                    }
                }
            }

            if (symbol.IsNormalAnonymousType() || symbol.IsAnonymousTypeProperty())
            {
                return symbol;
            }

            if (symbol is IParameterSymbol)
            {
                var parameter = (IParameterSymbol)symbol;
                if (parameter.ContainingSymbol is IMethodSymbol)
                {
                    var method = (IMethodSymbol)parameter.ContainingSymbol;
                    if (method.IsReducedExtension())
                    {
                        symbol = method.GetConstructedReducedFrom().Parameters[parameter.Ordinal + 1];
                    }
                }
            }

            return symbol == null ? null : symbol.OriginalDefinition;
        }

        public static bool IsFunctionValue(this ISymbol symbol)
        {
            return symbol is ILocalSymbol && ((ILocalSymbol)symbol).IsFunctionValue;
        }

        public static bool IsThisParameter(this ISymbol symbol)
        {
            return symbol != null && symbol.Kind == SymbolKind.Parameter && ((IParameterSymbol)symbol).IsThis;
        }

        public static ISymbol ConvertThisParameterToType(this ISymbol symbol)
        {
            if (symbol.IsThisParameter())
            {
                return ((IParameterSymbol)symbol).Type;
            }

            return symbol;
        }

        public static bool IsAttribute(this ISymbol symbol)
        {
            var typeSymbol = symbol as ITypeSymbol;

            return typeSymbol != null
                ? typeSymbol.IsAttribute()
                : false;
        }

        public static bool IsStaticType(this ISymbol symbol)
        {
            return symbol != null && symbol.Kind == SymbolKind.NamedType && symbol.IsStatic;
        }

        public static bool IsNamespace(this ISymbol symbol)
        {
            return symbol != null && symbol.Kind == SymbolKind.Namespace;
        }

        public static Accessibility ComputeResultantAccessibility(this ISymbol symbol, ITypeSymbol finalDestination)
        {
            if (symbol == null)
            {
                return Accessibility.Private;
            }

            switch (symbol.DeclaredAccessibility)
            {
                default:
                    return symbol.DeclaredAccessibility;
                case Accessibility.ProtectedAndInternal:
                    return symbol.ContainingAssembly.GivesAccessTo(finalDestination.ContainingAssembly)
                        ? Accessibility.ProtectedAndInternal
                        : Accessibility.Internal;
                case Accessibility.ProtectedOrInternal:
                    return symbol.ContainingAssembly.GivesAccessTo(finalDestination.ContainingAssembly)
                        ? Accessibility.ProtectedOrInternal
                        : Accessibility.Protected;
            }
        }

        /// <returns>
        /// Returns true if symbol is a local variable and its declaring syntax node is 
        /// after the current position, false otherwise (including for non-local symbols)
        /// </returns>
        public static bool IsInaccessibleLocal(this ISymbol symbol, int position)
        {
            if (symbol.Kind != SymbolKind.Local)
            {
                return false;
            }

            // Implicitly declared locals (with Option Explicit Off in VB) are scoped to the entire
            // method and should always be considered accessible from within the same method.
            if (symbol.IsImplicitlyDeclared)
            {
                return false;
            }

            var declarationSyntax = symbol.DeclaringSyntaxReferences.Select(r => r.GetSyntax()).FirstOrDefault();
            return declarationSyntax != null && position < declarationSyntax.SpanStart;
        }

        public static bool IsEventAccessor(this ISymbol symbol)
        {
            var method = symbol as IMethodSymbol;
            return method != null &&
                (method.MethodKind == MethodKind.EventAdd ||
                 method.MethodKind == MethodKind.EventRaise ||
                 method.MethodKind == MethodKind.EventRemove);
        }

        public static ITypeSymbol GetSymbolType(this ISymbol symbol)
        {
            var localSymbol = symbol as ILocalSymbol;
            if (localSymbol != null)
            {
                return localSymbol.Type;
            }

            var fieldSymbol = symbol as IFieldSymbol;
            if (fieldSymbol != null)
            {
                return fieldSymbol.Type;
            }

            var propertySymbol = symbol as IPropertySymbol;
            if (propertySymbol != null)
            {
                return propertySymbol.Type;
            }

            var parameterSymbol = symbol as IParameterSymbol;
            if (parameterSymbol != null)
            {
                return parameterSymbol.Type;
            }

            var aliasSymbol = symbol as IAliasSymbol;
            if (aliasSymbol != null)
            {
                return aliasSymbol.Target as ITypeSymbol;
            }

            return symbol as ITypeSymbol;
        }

        /// <summary>
        /// If the <paramref name="symbol"/> is a method symbol, returns True if the method's return type is "awaitable".
        /// If the <paramref name="symbol"/> is a type symbol, returns True if that type is "awaitable".
        /// An "awaitable" is any type that exposes a GetAwaiter method which returns a valid "awaiter". This GetAwaiter method may be an instance method or an extension method.
        /// </summary>
        public static bool IsAwaitable(this ISymbol symbol, SemanticModel semanticModel, int position)
        {
            IMethodSymbol methodSymbol = symbol as IMethodSymbol;
            ITypeSymbol typeSymbol = null;

            if (methodSymbol == null)
            {
                typeSymbol = symbol as ITypeSymbol;
                if (typeSymbol == null)
                {
                    return false;
                }
            }
            else
            {
                if (methodSymbol.ReturnType == null)
                {
                    return false;
                }

                // dynamic
                if (methodSymbol.ReturnType.TypeKind == TypeKind.DynamicType &&
                    methodSymbol.MethodKind != MethodKind.BuiltinOperator)
                {
                    return true;
                }
            }

            // otherwise: needs valid GetAwaiter
            var potentialGetAwaiters = semanticModel.LookupSymbols(position, 
                                                                   container: typeSymbol ?? methodSymbol.ReturnType.OriginalDefinition, 
                                                                   name: WellKnownMemberNames.GetAwaiter, 
                                                                   includeReducedExtensionMethods: true);
            var getAwaiters = potentialGetAwaiters.OfType<IMethodSymbol>().Where(x => !x.Parameters.Any());
            return getAwaiters.Any(VerifyGetAwaiter);
        }

        private static bool VerifyGetAwaiter(IMethodSymbol getAwaiter)
        {
            var returnType = getAwaiter.ReturnType;
            if (returnType == null)
            {
                return false;
            }

            // bool IsCompleted { get }
            if (!returnType.GetMembers().OfType<IPropertySymbol>().Any(p => p.Name == WellKnownMemberNames.IsCompleted && p.Type.SpecialType == SpecialType.System_Boolean && p.GetMethod != null))
            {
                return false;
            }

            var methods = returnType.GetMembers().OfType<IMethodSymbol>();

            // NOTE: (vladres) The current version of C# Spec, �7.7.7.3 'Runtime evaluation of await expressions', requires that
            // NOTE: the interface method INotifyCompletion.OnCompleted or ICriticalNotifyCompletion.UnsafeOnCompleted is invoked
            // NOTE: (rather than any OnCompleted method conforming to a certain pattern).
            // NOTE: Should this code be updated to match the spec?

            // void OnCompleted(Action) 
            // Actions are delegates, so we'll just check for delegates.
            if (!methods.Any(x => x.Name == WellKnownMemberNames.OnCompleted && x.ReturnsVoid && x.Parameters.Length == 1 && x.Parameters.First().Type.TypeKind == TypeKind.Delegate))
            {
                return false;
            }

            // void GetResult() || T GetResult()
            return methods.Any(m => m.Name == WellKnownMemberNames.GetResult && !m.Parameters.Any());
        }

        public static ITypeSymbol InferAwaitableReturnType(this ISymbol symbol, SemanticModel semanticModel, int position)
        {
            var methodSymbol = symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return null;
            }

            var returnType = methodSymbol.ReturnType;
            if (returnType == null)
            {
                return null;
            }

            var potentialGetAwaiters = semanticModel.LookupSymbols(position, container: returnType, name: WellKnownMemberNames.GetAwaiter, includeReducedExtensionMethods: true);
            var getAwaiters = potentialGetAwaiters.OfType<IMethodSymbol>().Where(x => !x.Parameters.Any());
            if (!getAwaiters.Any())
            {
                return null;
            }

            var getResults = getAwaiters.SelectMany(g => semanticModel.LookupSymbols(position, container: g.ReturnType, name: WellKnownMemberNames.GetResult));

            var getResult = getResults.OfType<IMethodSymbol>().FirstOrDefault(g => !g.IsStatic);
            if (getResult == null)
            {
                return null;
            }

            return getResult.ReturnType;
        }

        private static IEnumerable<T> RemoveOverriddenSymbolsWithinSet<T>(this IEnumerable<T> symbols) where T : ISymbol
        {
            HashSet<ISymbol> overriddenSymbols = new HashSet<ISymbol>();

            foreach (var symbol in symbols)
            {
                if (symbol.OverriddenMember() != null && !overriddenSymbols.Contains(symbol.OverriddenMember()))
                {
                    overriddenSymbols.Add(symbol.OverriddenMember());
                }
            }

            return symbols.Where(s => !overriddenSymbols.Contains(s));
        }

        private static readonly Dictionary<string, string> OperatorNameTable = new Dictionary<string, string>()
        {
            { WellKnownMemberNames.AdditionOperatorName, "+" },
            { WellKnownMemberNames.BitwiseAndOperatorName, "&" },
            { WellKnownMemberNames.BitwiseOrOperatorName, "|" },
            { WellKnownMemberNames.DecrementOperatorName, "--" },
            { WellKnownMemberNames.DivisionOperatorName, "/" },
            { WellKnownMemberNames.EqualityOperatorName, "==" },
            { WellKnownMemberNames.ExclusiveOrOperatorName, "^" },
            { WellKnownMemberNames.FalseOperatorName, "false" },
            { WellKnownMemberNames.GreaterThanOperatorName, ">" },
            { WellKnownMemberNames.GreaterThanOrEqualOperatorName, ">=" },
            { WellKnownMemberNames.IncrementOperatorName, "++" },
            { WellKnownMemberNames.InequalityOperatorName, "!=" },
            { WellKnownMemberNames.LessThanOperatorName, "<" },
            { WellKnownMemberNames.LessThanOrEqualOperatorName, "<=" },
            { WellKnownMemberNames.LeftShiftOperatorName, "<<" },
            { WellKnownMemberNames.LogicalNotOperatorName, "!" },
            { WellKnownMemberNames.UnsignedLeftShiftOperatorName, "<<" },
            { WellKnownMemberNames.ModulusOperatorName, "%" },
            { WellKnownMemberNames.MultiplyOperatorName, "*" },
            { WellKnownMemberNames.OnesComplementOperatorName, "~" },
            { WellKnownMemberNames.RightShiftOperatorName, ">>" },
            { WellKnownMemberNames.UnsignedRightShiftOperatorName, ">>" },
            { WellKnownMemberNames.SubtractionOperatorName, "-" },
            { WellKnownMemberNames.TrueOperatorName, "true" },
            { WellKnownMemberNames.UnaryNegationOperatorName, "-" },
            { WellKnownMemberNames.UnaryPlusOperatorName, "+" },
        };

        public static string GetOperatorTokenText(this ISymbol method)
        {
            string value = null;
            OperatorNameTable.TryGetValue(method.Name, out value);
            return value;
        }
    }
}