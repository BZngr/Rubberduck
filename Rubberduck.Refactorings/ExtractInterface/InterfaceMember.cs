﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Rubberduck.Parsing.Grammar;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Refactorings.ImplementInterface;

namespace Rubberduck.Refactorings.ExtractInterface
{
    public class InterfaceMember : INotifyPropertyChanged
    {
        public Declaration Member { get; }
        public IEnumerable<Parameter> MemberParams { get; }
        private string Type { get; }
        private string MemberType { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public string Identifier { get; }

        public string FullMemberSignature
        {
            get
            {
                var signature = $"{MemberType} {Member.IdentifierName}({string.Join(", ", MemberParams)})";

                return Type == null ? signature : $"{signature} As {Type}";
            }
        }

        public InterfaceMember(Declaration member)
        {
            Member = member;
            Identifier = member.IdentifierName;
            Type = member.AsTypeName;
            
            GetMethodType();

            if (member is IParameterizedDeclaration memberWithParams)
            {
                MemberParams = memberWithParams.Parameters
                    .OrderBy(parameter => parameter.Selection)
                    .Select(parameter => new Parameter(parameter))
                    .ToList();
            }
            else
            {
                MemberParams = new List<Parameter>();
            }

            if (MemberType == "Property Get")
            {
                MemberParams = MemberParams.Take(MemberParams.Count() - 1);
            }
        }

        private void GetMethodType()
        {
            var context = Member.Context;

            if (context is VBAParser.SubStmtContext)
            {
                MemberType = Tokens.Sub;
            }

            if (context is VBAParser.FunctionStmtContext)
            {
                MemberType = Tokens.Function;
            }

            if (context is VBAParser.PropertyGetStmtContext)
            {
                MemberType = $"{Tokens.Property} {Tokens.Get}";
            }

            if (context is VBAParser.PropertyLetStmtContext)
            {
                MemberType = $"{Tokens.Property} {Tokens.Let}";
            }

            if (context is VBAParser.PropertySetStmtContext)
            {
                MemberType = $"{Tokens.Property} {Tokens.Set}";
            }
        }

        public string Body => string.Format("Public {0}{1}End {2}{1}", FullMemberSignature, Environment.NewLine, MemberType.Split(' ').First());
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
