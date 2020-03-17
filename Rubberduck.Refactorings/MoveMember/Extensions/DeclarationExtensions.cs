﻿using Rubberduck.Parsing;
using Rubberduck.Parsing.Symbols;
using System.Collections.Generic;
using System.Linq;

namespace Rubberduck.Refactorings.MoveMember.Extensions
{

    public static class DeclarationExtensions
    {
        public static bool IsVariable(this Declaration declaration)
            => declaration.DeclarationType.HasFlag(DeclarationType.Variable);

        public static bool IsField(this Declaration declaration)
            => declaration.IsVariable() && !declaration.IsLocalVariable();

        public static bool IsLocalVariable(this Declaration declaration)
            => declaration.IsVariable() && declaration.ParentDeclaration.IsMember();

        public static bool IsModuleConstant(this Declaration declaration)
            => declaration.IsConstant() && !declaration.IsLocalConstant();

        public static bool IsConstant(this Declaration declaration)
            => declaration.DeclarationType.HasFlag(DeclarationType.Constant);

        public static bool IsLocalConstant(this Declaration declaration)
            => declaration.IsConstant() && declaration.ParentDeclaration.IsMember();

        public static bool HasPrivateAccessibility(this Declaration declaration)
            => declaration.Accessibility.Equals(Accessibility.Private);

        public static bool IsMember(this Declaration declaration)
            => declaration.DeclarationType.HasFlag(DeclarationType.Member);

        public static bool IsLifeCycleHandler(this Declaration declaration)
            => declaration.DeclarationType.HasFlag(DeclarationType.Member)
                    && (declaration.IdentifierName.Equals("Class_Initialize")
                        || declaration.IdentifierName.Equals("Class_Terminate"));

        public static IEnumerable<IdentifierReference> AllReferences(this IEnumerable<Declaration> declarations)
        {
            return from dec in declarations
                   from reference in dec.References
                   select reference;
        }

        public static bool ContainsParentScopesForAll(this IEnumerable<Declaration> containing, IEnumerable<IdentifierReference> references) 
            => references.All(rf => containing.Contains(rf.ParentScoping));

        public static bool ContainsParentScopeForAnyReference(this IEnumerable<Declaration> containing, IEnumerable<IdentifierReference> references) 
            => references.Any(rf => containing.Contains(rf.ParentScoping));
    }
}
