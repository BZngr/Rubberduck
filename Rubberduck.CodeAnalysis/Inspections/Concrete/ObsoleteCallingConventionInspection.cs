﻿using Rubberduck.Inspections.Abstract;
using Rubberduck.Parsing;
using Rubberduck.Parsing.Grammar;
using Rubberduck.Parsing.Inspections.Abstract;
using Rubberduck.Parsing.VBA;
using Rubberduck.Parsing.VBA.DeclarationCaching;
using Rubberduck.Resources.Inspections;

namespace Rubberduck.Inspections.Inspections.Concrete
{
    /// <summary>
    /// Warns about 'Declare' statements that are using the obsolete/unsupported 'CDecl' calling convention on Windows.
    /// </summary>
    /// <why>
    /// The CDecl calling convention is only implemented in VBA for Mac; if Rubberduck can see it (Rubberduck only runs on Windows),
    /// then the declaration is using an unsupported (no-op) calling convention on Windows.
    /// </why>
    /// <example hasResults="true">
    /// <![CDATA[
    /// Private Declare Sub Beep CDecl Lib "kernel32" (dwFreq As Any, dwDuration As Any)
    /// ]]>
    /// </example>
    /// <example hasResults="false">
    /// <![CDATA[
    /// Private Declare Sub Beep Lib "kernel32" (dwFreq As Any, dwDuration As Any)
    /// ]]>
    /// </example>
    public sealed class ObsoleteCallingConventionInspection : ParseTreeInspectionBase<VBAParser.DeclareStmtContext>
    {
        public ObsoleteCallingConventionInspection(IDeclarationFinderProvider declarationFinderProvider)
            : base(declarationFinderProvider)
        {
            ContextListener = new ObsoleteCallingConventionListener();
        }

        protected override IInspectionListener<VBAParser.DeclareStmtContext> ContextListener { get; }

        protected override string ResultDescription(QualifiedContext<VBAParser.DeclareStmtContext> context)
        {
            var identifierName = ((VBAParser.DeclareStmtContext) context.Context).identifier().GetText();
            return string.Format(
                InspectionResults.ObsoleteCallingConventionInspection,
                identifierName);
        }

        protected override bool IsResultContext(QualifiedContext<VBAParser.DeclareStmtContext> context, DeclarationFinder finder)
        {
            return ((VBAParser.DeclareStmtContext)context.Context).CDECL() != null;
        }

        public class ObsoleteCallingConventionListener : InspectionListenerBase<VBAParser.DeclareStmtContext>
        {
            public override void ExitDeclareStmt(VBAParser.DeclareStmtContext context)
            {
                SaveContext(context);
            }
        }
    }
}
