﻿using System.Linq;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Parsing.VBA;
using Rubberduck.Refactorings.MoveCloserToUsage;
using Rubberduck.UI.Command.Refactorings.Notifiers;
using Rubberduck.VBEditor.Utility;

namespace Rubberduck.UI.Command.Refactorings
{
    public class RefactorMoveCloserToUsageCommand : RefactorCodePaneCommandBase
    {
        private readonly RubberduckParserState _state;
        private readonly ISelectedDeclarationService _selectedDeclarationService;

        public RefactorMoveCloserToUsageCommand(
            MoveCloserToUsageRefactoring refactoring, 
            MoveCloserToUsageFailedNotifier moveCloserToUsageFailedNotifier, 
            RubberduckParserState state, 
            ISelectionService selectionService,
            ISelectedDeclarationService selectedDeclarationService)
            :base(refactoring, moveCloserToUsageFailedNotifier, selectionService, state)
        {
            _state = state;
            _selectedDeclarationService = selectedDeclarationService;

            AddToCanExecuteEvaluation(SpecializedEvaluateCanExecute);
        }

        private bool SpecializedEvaluateCanExecute(object parameter)
        {
            var target = GetTarget();

            return target != null
                   && !_state.IsNewOrModified(target.QualifiedModuleName)
                   && (target.DeclarationType == DeclarationType.Variable
                       || target.DeclarationType == DeclarationType.Constant)
                   && target.References.Any();
        }

        private Declaration GetTarget()
        {
            return _selectedDeclarationService.SelectedDeclaration();
        }
    }
}
