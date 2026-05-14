using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CSFileParser
{
    internal static class DocParser
    {
        internal static void ParseDoc(string fullPath)
        {
            var code = File.ReadAllText(fullPath);
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetCompilationUnitRoot();

            var members = new List<object>();

            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var m in methods)
            {
                members.Add(new
                {
                    type = "method",
                    name = m.Identifier.Text,
                    fullCode = m.ToFullString().Trim()
                });
            }

            var props = root.DescendantNodes().OfType<PropertyDeclarationSyntax>();
            foreach (var p in props)
            {
                members.Add(new
                {
                    type = "property",
                    name = p.Identifier.Text,
                    fullCode = p.ToFullString().Trim()
                });
            }
            var path = Path.Combine(Path.GetDirectoryName(fullPath), "extracted_members.json");
            File.WriteAllText(path, JsonSerializer.Serialize(members));
        }
    }
}

