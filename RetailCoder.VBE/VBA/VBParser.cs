﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.Vbe.Interop;
using Rubberduck.Extensions;
using Rubberduck.Inspections;
using Rubberduck.VBA.Grammar;
using Rubberduck.VBA.Nodes;

namespace Rubberduck.VBA
{
    public class VBParser : IRubberduckParser
    {
        /// <summary>
        /// An overload for the COM API.
        /// </summary>
        public INode Parse(string projectName, string componentName, string code)
        {
            var result = Parse(code);
            var walker = new ParseTreeWalker();
            
            var listener = new NodeBuildingListener(projectName, componentName);
            walker.Walk(listener, result);

            return listener.Root;
        }

        public IParseTree Parse(string code)
        {
            var input = new AntlrInputStream(code);
            var lexer = new VisualBasic6Lexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new VisualBasic6Parser(tokens);
            
            var result = parser.startRule();
            return result;
        }

        public IEnumerable<VBComponentParseResult> Parse(VBProject project)
        {
            return project.VBComponents.Cast<VBComponent>()
                          .Select(component => new VBComponentParseResult(component, 
                                               Parse(component.CodeModule.Lines()), ParseComments(component)));
        }

        public async Task<IEnumerable<VBComponentParseResult>> ParseAsync(VBProject project)
        {
            return await Task.Run(() => project.VBComponents.Cast<VBComponent>()
                .AsParallel()
                .Select(component =>
                {
                    var lines = Parse(component.CodeModule.Lines());
                    var comments = ParseComments(component);
                    return new VBComponentParseResult(component, lines, comments);
                }));
        }

        public async Task<VBComponentParseResult> ParseAsync(VBComponent component)
        {
            var result = await Task.Run(() => Parse(component.CodeModule.Lines()));
            var comments = await Task.Run(() => ParseComments(component));
            return new VBComponentParseResult(component, result, comments);
        }

        public IEnumerable<CommentNode> ParseComments(VBComponent component)
        {
            var code = component.CodeModule.Code();
            var qualifiedName = new QualifiedModuleName(component.Collection.Parent.Name, component.Name);

            var commentBuilder = new StringBuilder();
            var continuing = false;

            var startLine = 0;
            var startColumn = 0;

            for (var i = 0; i < code.Length; i++)
            {
                var line = code[i];                
                var index = 0;

                if (continuing || line.HasComment(out index))
                {
                    startLine = continuing ? startLine : i;
                    startColumn = continuing ? startColumn : index;

                    var commentLength = line.Length - index;

                    continuing = line.EndsWith("_");
                    if (!continuing)
                    {
                        commentBuilder.Append(line.Substring(index, commentLength).TrimStart());
                        var selection = new Selection(startLine + 1, startColumn + 1, i + 1, line.Length);

                        var result = new CommentNode(commentBuilder.ToString(), new QualifiedSelection(qualifiedName, selection));
                        commentBuilder.Clear();
                        
                        yield return result;
                    }
                    else
                    {
                        // ignore line continuations in comment text:
                        commentBuilder.Append(line.Substring(index, commentLength).TrimStart()); 
                    }
                }
            }
        }
    }
}
