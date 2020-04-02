﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Resources;
using Rubberduck.Refactorings.MoveToFolder;

namespace Rubberduck.UI.Refactorings.MoveToFolder
{
    public class MoveMultipleToFolderViewModel : RefactoringViewModelBase<MoveMultipleToFolderModel>
    {
        public MoveMultipleToFolderViewModel(MoveMultipleToFolderModel model) 
            : base(model)
        {}

        private ICollection<ModuleDeclaration> Targets => Model.Targets;

        public string Instructions
        {
            get
            {
                if (Targets == null || !Targets.Any())
                {
                    return string.Empty;
                }

                if (Targets.Count == 1)
                {
                    var target = Targets.First();
                    var moduleName = target.IdentifierName;
                    var declarationType = RubberduckUI.ResourceManager.GetString("DeclarationType_" + target.DeclarationType, CultureInfo.CurrentUICulture);
                    var currentFolder = target.CustomFolder;
                    return string.Format(RubberduckUI.MoveToFolderDialog_InstructionsLabelText, declarationType, moduleName, currentFolder);
                }

                return string.Format(RubberduckUI.MoveMultipleToFolderDialog_InstructionsLabelText);
            }
        }

        public string NewFolder
        {
            get => Model.TargetFolder;
            set
            {
                Model.TargetFolder = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValidFolder));
            }
        }
        
        public bool IsValidFolder => Targets != null 
                                     && Targets.Any()
                                     && NewFolder != null 
                                     && !NewFolder.Any(char.IsControl);
        
        protected override void DialogOk()
        {
            if (Targets == null || !Targets.Any())
            {
                base.DialogCancel();
            }
            else
            {
                base.DialogOk();
            }
        }
    }
}
