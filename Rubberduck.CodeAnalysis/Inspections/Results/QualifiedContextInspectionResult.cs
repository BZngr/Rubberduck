﻿using System.Collections.Generic;
using Rubberduck.Inspections.Abstract;
using Rubberduck.Parsing;
using Rubberduck.Parsing.Inspections.Abstract;
using Rubberduck.VBEditor;

namespace Rubberduck.Inspections.Results
{
    public class QualifiedContextInspectionResult : InspectionResultBase
    {
        public QualifiedContextInspectionResult(
            IInspection inspection, 
            string description, 
            QualifiedContext context,
            ICollection<string> disabledQuickFixes = null) :
            base(inspection,
                 description,
                 context.ModuleName,
                 context.Context,
                 null,
                 new QualifiedSelection(context.ModuleName, context.Context.GetSelection()),
                 context.MemberName,
                 disabledQuickFixes)
        {}
    }

    public class QualifiedContextInspectionResult<T> : QualifiedContextInspectionResult, IWithInspectionResultProperties<T>
    {
        public QualifiedContextInspectionResult(
            IInspection inspection, 
            string description, 
            QualifiedContext context,
            T properties,
            ICollection<string> disabledQuickFixes = null) :
            base(
                inspection,
                description,
                context,
                disabledQuickFixes)
        {
            Properties = properties;
        }

        public T Properties { get; }
    }
}
