using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using Nudity.Models;
using Nudity.Utils;

namespace Nudity.Generator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        private readonly TemplateRenderer _templateRenderer = new();
        
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            //System.Diagnostics.Debugger.Launch();
#endif
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;

            var symbolComparer = SymbolEqualityComparer.Default;
            var compilation = context.Compilation;
            var objectFullName = typeof(object).FullName;
            var objectTypeSymbol = compilation.GetTypeByMetadataName(objectFullName)!;

            var methodsModel = receiver
                .MemberAccesses
                .Select(m =>
                {
                    var semanticModel = compilation.GetSemanticModel(m.SyntaxTree);
                    var childNode = m.ChildNodes().FirstOrDefault();
                    return childNode is null ? null : (TypeInfo?)semanticModel.GetTypeInfo(childNode);
                })
                .Select(ti => ti?.Type)
                .Distinct(symbolComparer)
                .OfType<INamedTypeSymbol>()
                .Where(ts => ts is not IErrorTypeSymbol)
                .Where(ts => !symbolComparer.Equals(ts, objectTypeSymbol))
                .Select(ts =>
                {
                    var wrapperFullTypeName = GenerateTypeWrapper(context, ts);
                    return new AsExposedModel
                    {
                        ArgumentTypeName = ts.ToString(),
                        ReturnTypeName = wrapperFullTypeName
                    };
                })
                .ToArray();
            
            var model = new ExposedObjectExtensionsModel { Methods = methodsModel };
            var source = _templateRenderer.Render(TemplatePaths.ExposedObjectExtensions, model);
            context.AddSource(SourceFileNames.ExposedObjectExtensions, source);
        }

        private string GenerateTypeWrapper(GeneratorExecutionContext context, ITypeSymbol typeSymbol)
        {
            var typeSymbolName = typeSymbol.Name;
            var originalFullTypeName = typeSymbol.ToString();
            var wrapperNamespace = $"{typeSymbol.ContainingNamespace}.NudityWrappers";
            var wrapperFullTypeName = $"{wrapperNamespace}.{typeSymbol.Name}";

            var assemblySymbol = typeSymbol.ContainingAssembly;
            var metadataReference = context.Compilation.GetMetadataReference(assemblySymbol) as PortableExecutableReference;

            var fields = new StringBuilder();
            if (metadataReference == null)
            {
                var members = typeSymbol.GetMembers();
                
                foreach (var member in members.OfType<IFieldSymbol>().Where(m => !IsBackingField(m.Name)))
                {
                    if (member.Type is not INamedTypeSymbol namedTypeSymbol)
                        continue;
                    
                    fields.AppendLine($"        public {GenerateTypeIdentifier(namedTypeSymbol)} {member.Name};");
                }
            }
            else
            {
                var assemblyPath = metadataReference.FilePath!;

                if (assemblyPath.Contains(@"\packs\"))
                {
                    assemblyPath = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.7\" +
                                   Path.GetFileName(assemblyPath);
                }

                var decompiler = new CSharpDecompiler(assemblyPath, new DecompilerSettings());
                var decompiledType = decompiler.TypeSystem.FindType(new FullTypeName(originalFullTypeName));
                var members = decompiledType.GetMembers();
                
                foreach (var member in members.OfType<IField>().Where(m => !IsBackingField(m.Name)))
                {
                    var typeIdentifier = GenerateTypeIdentifier(member.Type);

                    fields.AppendLine($"       public {typeIdentifier} {member.Name}");
                    fields.AppendLine($"       {{");
                    fields.AppendLine($"           get {{ return ({typeIdentifier})typeof({originalFullTypeName}).GetField(\"{member.Name}\", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_wrappedObject); }}");
                    fields.AppendLine($"           set {{ typeof({originalFullTypeName}).GetField(\"{member.Name}\", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_wrappedObject, value); }}");
                    fields.AppendLine($"       }}");
                    fields.AppendLine();
                }
            }

            var source = 
@$"{AutoGeneratedDiscalimer}

namespace {wrapperNamespace}
{{
    internal class {typeSymbolName}
    {{
        private readonly object _wrappedObject;

        public {typeSymbolName}(object wrappedObject)
        {{
            _wrappedObject = wrappedObject;
        }}

{fields}
    }}
}}";
            var invalidChars = Path.GetInvalidFileNameChars();
            var hintName = originalFullTypeName;
            
            foreach (var invalidChar in invalidChars)
            {
                hintName = hintName.Replace(invalidChar, '_');
            }
            
            context.AddSource(hintName, source);
            return wrapperFullTypeName;
        }

        private const string AutoGeneratedDiscalimer = "// <auto-generated />";

        private static bool IsBackingField(string fieldName)
        {
            return fieldName.StartsWith("<");
        }

        private static string GenerateTypeIdentifier(INamedTypeSymbol type)
        {
            var fullTypeName = GetFullTypeName(type);
            
            if (type.IsGenericType)
            {
                var genericTypeNames = string.Join(", ", type.TypeArguments.Select(GetFullTypeName));
                return $"{fullTypeName}<{genericTypeNames}>";
            }
            
            return fullTypeName;
        }

        private static string GenerateTypeIdentifier(IType type)
        {
            var fullTypeName = type.FullName;

            if (type.TypeArguments.Count > 0)
            {
                var genericTypeNames = string.Join(", ", type.TypeArguments.Select(t => t.FullName));
                return $"{fullTypeName}<{genericTypeNames}>";
            }
            
            return fullTypeName;
        }

        private static string GetFullTypeName(ITypeSymbol type)
        {
            return $"{type.ContainingNamespace}.{type.Name}";
        }
    }
}