using System.Collections.Generic;
using System.Linq;
using Rubberduck.Inspections.Abstract;
using Rubberduck.Inspections.Results;
using Rubberduck.Parsing;
using Rubberduck.Parsing.Annotations;
using Rubberduck.Parsing.Inspections.Abstract;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Resources.Inspections;
using Rubberduck.Parsing.VBA;
using Rubberduck.Parsing.VBA.DeclarationCaching;
using Rubberduck.VBEditor;

namespace Rubberduck.Inspections.Concrete
{
    /// <summary>
    /// Warns about a malformed Rubberduck annotation that is missing one or more arguments.
    /// </summary>
    /// <why>
    /// Some annotations require arguments; if the required number of arguments isn't specified, the annotation is nothing more than an obscure comment.
    /// </why>
    /// <example hasResults="true">
    /// <![CDATA[
    /// '@Folder
    /// '@ModuleDescription
    /// Option Explicit
    /// ' ...
    /// ]]>
    /// </example>
    /// <example hasResults="false">
    /// <![CDATA[
    /// '@Folder("MyProject.XYZ")
    /// '@ModuleDescription("This module does XYZ")
    /// Option Explicit
    /// ' ...
    /// ]]>
    /// </example>
    public sealed class MissingAnnotationArgumentInspection : InspectionBase
    {
        public MissingAnnotationArgumentInspection(IDeclarationFinderProvider declarationFinderProvider)
            : base(declarationFinderProvider)
        {}

        protected override IEnumerable<IInspectionResult> DoGetInspectionResults(DeclarationFinder finder)
        {
            return finder.UserDeclarations(DeclarationType.Module)
                .Where(module => module != null)
                .SelectMany(module => DoGetInspectionResults(module.QualifiedModuleName, finder))
                .ToList();
        }

        protected override IEnumerable<IInspectionResult> DoGetInspectionResults(QualifiedModuleName module, DeclarationFinder finder)
        {
            var objectionableAnnotations = finder.FindAnnotations(module)
                .Where(IsResultAnnotation);

            return objectionableAnnotations
                .Select(InspectionResult)
                .ToList();
        }

        private static bool IsResultAnnotation(IParseTreeAnnotation pta)
        {
            return pta.Annotation.RequiredArguments > pta.AnnotationArguments.Count;
        }

        private IInspectionResult InspectionResult(IParseTreeAnnotation pta)
        {
            var qualifiedContext = new QualifiedContext(pta.QualifiedSelection.QualifiedName, pta.Context);
            return new QualifiedContextInspectionResult(
                this,
                ResultDescription(pta),
                qualifiedContext);
        }

        private static string ResultDescription(IParseTreeAnnotation pta)
        {
            return string.Format(
                InspectionResults.MissingAnnotationArgumentInspection,
                pta.Annotation.Name);
        }
    }
}
