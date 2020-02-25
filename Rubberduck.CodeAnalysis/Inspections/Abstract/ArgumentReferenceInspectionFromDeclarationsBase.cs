﻿using System.Collections.Generic;
using System.Linq;
using Rubberduck.Inspections.Abstract;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Parsing.VBA;
using Rubberduck.Parsing.VBA.DeclarationCaching;

namespace Rubberduck.Inspections.Inspections.Abstract
{
    public abstract class ArgumentReferenceInspectionFromDeclarationsBase : IdentifierReferenceInspectionFromDeclarationsBase
    {
        protected ArgumentReferenceInspectionFromDeclarationsBase(RubberduckParserState state) 
            : base(state) { }

        protected abstract bool IsUnsuitableArgument(ArgumentReference reference, DeclarationFinder finder);

        protected override IEnumerable<IdentifierReference> ObjectionableReferences(DeclarationFinder finder)
        {
            return ObjectionableDeclarations(finder)
                .OfType<ParameterDeclaration>()
                .SelectMany(parameter => parameter.ArgumentReferences);
        }

        protected override bool IsResultReference(IdentifierReference reference, DeclarationFinder finder)
        {
            if (!(reference is ArgumentReference argumentReference))
            {
                return false;
            }

            return IsUnsuitableArgument(argumentReference, finder);
        }
    }

    public abstract class ArgumentReferenceInspectionFromDeclarationsBase<T> : IdentifierReferenceInspectionFromDeclarationsBase<T>
    {
        protected ArgumentReferenceInspectionFromDeclarationsBase(RubberduckParserState state)
            : base(state) { }

        protected abstract (bool isResult, T properties) IsUnsuitableArgumentWithAdditionalProperties(ArgumentReference reference, DeclarationFinder finder);

        protected override IEnumerable<IdentifierReference> ObjectionableReferences(DeclarationFinder finder)
        {
            return ObjectionableDeclarations(finder)
                .OfType<ParameterDeclaration>()
                .SelectMany(parameter => parameter.ArgumentReferences);
        }

        protected override (bool isResult, T properties) IsResultReferenceWithAdditionalProperties(IdentifierReference reference, DeclarationFinder finder)
        {
            if (!(reference is ArgumentReference argumentReference))
            {
                return (false, default);
            }

            return IsUnsuitableArgumentWithAdditionalProperties(argumentReference, finder);
        }
    }
}
