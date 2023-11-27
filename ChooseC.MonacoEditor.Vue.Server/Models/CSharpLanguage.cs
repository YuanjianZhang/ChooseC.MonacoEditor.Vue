using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis;

namespace ChooseC.MonacoEditor.Api.Models
{
    public class CSharpLanguage : ILanguageService
    {
        private static readonly LanguageVersion MaxLanguageVersion = Enum
            .GetValues(typeof(LanguageVersion))
            .Cast<LanguageVersion>()
            .Max();

        public SyntaxTree ParseText(string sourceCode, SourceCodeKind kind)
        {
            var options = new CSharpParseOptions(kind: kind, languageVersion: MaxLanguageVersion);

            // Return a syntax tree of our source code
            return CSharpSyntaxTree.ParseText(sourceCode, options);
        }

        public Compilation CreateLibraryCompilation(string assemblyName, bool enableOptimisations)
        {
            throw new NotImplementedException();
        }
    }
}
