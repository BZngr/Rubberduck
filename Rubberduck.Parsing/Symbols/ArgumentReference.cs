﻿using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Rubberduck.Parsing.Annotations;
using Rubberduck.Parsing.Binding;
using Rubberduck.Parsing.Grammar;
using Rubberduck.VBEditor;
using Rubberduck.VBEditor.ComManagement;
using Rubberduck.VBEditor.SafeComWrappers.Abstract;

namespace Rubberduck.Parsing.Symbols
{
    public class ArgumentReference : IdentifierReference
    {
        internal ArgumentReference(
            QualifiedModuleName qualifiedName,
            Declaration parentScopingDeclaration,
            Declaration parentNonScopingDeclaration,
            string identifierName,
            Selection argumentSelection,
            ParserRuleContext context,
            VBAParser.ArgumentListContext argumentListContext,
            ArgumentListArgumentType argumentType,
            int argumentPosition,
            ParameterDeclaration referencedParameter,
            IEnumerable<IParseTreeAnnotation> annotations = null)
            : base(
                qualifiedName,
                parentScopingDeclaration,
                parentNonScopingDeclaration,
                identifierName,
                argumentSelection,
                context,
                referencedParameter,
                false,
                false,
                annotations)
        {
            ArgumentType = argumentType;
            ArgumentPosition = argumentPosition;
            ArgumentListContext = argumentListContext;
            NumberOfArguments = ArgumentListContext?.argument()?.Length ?? 0;
            ArgumentListSelection = argumentListContext?.GetSelection() ?? Selection.Empty;
        }

        public ArgumentListArgumentType ArgumentType { get; }
        public int ArgumentPosition { get; }
        public int NumberOfArguments { get; }
        public VBAParser.ArgumentListContext ArgumentListContext { get; }
        public Selection ArgumentListSelection { get; }

        public override (string context, Selection highlight) HighligthSelection(ICodeModule module)
        {
            var lines = module.GetLines(Selection.StartLine, Selection.LineCount).Split('\n');

            var line = lines[0]; // TODO think of something that makes sense for multiline
            var indent = line.TakeWhile((c, i) => c.Equals(' ') && i < Selection.StartColumn).Count();
            return (line.Trim(), new Selection(1, Math.Max(Selection.StartColumn - indent - 1, 1), 1, Math.Max(Selection.EndColumn - indent,1)).ToZeroBased());
        }

    }
}