using System.Linq;
using Rubberduck.Inspections.Abstract;
using Rubberduck.Resources.Inspections;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Parsing.VBA;
using Rubberduck.Inspections.Inspections.Extensions;
using Rubberduck.Parsing.VBA.DeclarationCaching;

namespace Rubberduck.Inspections.Concrete
{
    /// <summary>
    /// Locates 'Const' declarations that are never referenced.
    /// </summary>
    /// <why>
    /// Declarations that are never used should be removed.
    /// </why>
    /// <example hasResults="true">
    /// <![CDATA[
    /// Private Const foo As Long = 42
    ///
    /// Public Sub DoSomething()
    ///     ' no reference to 'foo' anywhere...
    /// End Sub
    /// ]]>
    /// </example>
    /// <example hasResults="false">
    /// <![CDATA[
    /// Private Const foo As Long = 42
    ///
    /// Public Sub DoSomething()
    ///     Debug.Print foo
    /// End Sub
    /// ]]>
    /// </example>
    public sealed class ConstantNotUsedInspection : DeclarationInspectionBase
    {
        public ConstantNotUsedInspection(RubberduckParserState state)
            : base(state, DeclarationType.Constant) { }

        protected override bool IsResultDeclaration(Declaration declaration, DeclarationFinder finder)
        {
            return declaration?.Context != null 
                   && !declaration.References.Any();
        }

        protected override string ResultDescription(Declaration declaration)
        {
            var declarationType = declaration.DeclarationType.ToLocalizedString();
            var declarationName = declaration.IdentifierName;
            return string.Format(
                InspectionResults.IdentifierNotUsedInspection,
                declarationType,
                declarationName);
        }
    }
}
