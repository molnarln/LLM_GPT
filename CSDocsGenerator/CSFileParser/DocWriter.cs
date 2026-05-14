using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace CSFileParser
{
    internal class DocWriter : CSharpSyntaxRewriter
    {
        private readonly Dictionary<string, string> _docs;
        private readonly string _csFilePath;

        public DocWriter(string jsonPath, string csFilePath)
        {
            _docs = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(jsonPath));
            _csFilePath = csFilePath;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            return AddDocToNode(node, node.Identifier.Text, base.VisitMethodDeclaration(node));
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            return AddDocToNode(node, node.Identifier.Text, base.VisitPropertyDeclaration(node));
        }

        private SyntaxNode AddDocToNode(SyntaxNode node, string name, SyntaxNode defaultResult)
        {
            if (_docs.ContainsKey(name))
            {
                var leadingTrivia = node.GetLeadingTrivia();
                var indentation = leadingTrivia.LastOrDefault(t => t.IsKind(SyntaxKind.WhitespaceTrivia));

                var cleanDoc = _docs[name].Replace("```xml", "").Replace("```", "").Trim();
                var lines = cleanDoc.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);

                var sb = new StringBuilder();
                sb.Append(Environment.NewLine);
                foreach (var line in lines)
                {
                    sb.Append(indentation.ToFullString());
                    sb.AppendLine(line.Trim());
                }

                sb.Append(indentation.ToFullString());

                var trivia = SyntaxFactory.ParseLeadingTrivia(sb.ToString());

                return node.WithLeadingTrivia(trivia);
            }

            return defaultResult;
        }

        public void ReWrite()
        {
            string sourceCode = File.ReadAllText(_csFilePath);

            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceCode);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            var resultNode = Visit(root);

            File.WriteAllText(Path.Combine(Path.GetDirectoryName(_csFilePath), "NewFileWithDocs.cs"), resultNode.ToFullString());

            Console.WriteLine("A dokumentáció visszaírása sikeresen megtörtént!");
        }
    }
}
