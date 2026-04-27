using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Shared.Roslyn;

[Generator]
public class RequestComponentGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var structDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is StructDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!.Value);

        context.RegisterSourceOutput(structDeclarations, static (spc, source) => Execute(spc, source));
    }

    private static StructSymbolContext? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var structDeclaration = (StructDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(structDeclaration) as INamedTypeSymbol;
        
        if (symbol is null) return null;

        var attribute = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "RequestComponentAttribute");

        if (attribute is null) return null;

        return new StructSymbolContext(symbol);
    }

    private static void Execute(SourceProductionContext context, StructSymbolContext info)
    {
        var sb = new StringBuilder();
        var ns = info.Symbol.ContainingNamespace.IsGlobalNamespace ? "" : info.Symbol.ContainingNamespace.ToDisplayString();
        var structName = info.Symbol.Name;

        sb.AppendLine("using MessagePack;");
        sb.AppendLine("using System;");
        sb.AppendLine();
        if (!string.IsNullOrEmpty(ns)) sb.AppendLine($"namespace {ns};");
        sb.AppendLine();
        sb.AppendLine($"public partial struct {structName}");
        sb.AppendLine("{");
        
        sb.AppendLine($"    public static {structName} Deserialize(ref MessagePackReader reader)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var result = new {structName}();");

        var fields = info.Symbol.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f.DeclaredAccessibility == Accessibility.Public);

        foreach (var field in fields)
        {
            string readCall = GetReadMethod(field.Type);
            sb.AppendLine($"        result.{field.Name} = {readCall};");
        }

        sb.AppendLine("        return result;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{structName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static string GetReadMethod(ITypeSymbol type)
    {
        return type.SpecialType switch
        {
            SpecialType.System_Int32 => "reader.ReadInt32()",
            SpecialType.System_Single => "reader.ReadSingle()",
            SpecialType.System_Boolean => "reader.ReadBoolean()",
            SpecialType.System_String => "reader.ReadString()",
            _ => $"MessagePackSerializer.Deserialize<{type.ToDisplayString()}>(ref reader)"
        };
    }

    private readonly struct StructSymbolContext
    {
        public INamedTypeSymbol Symbol { get; }
        public StructSymbolContext(INamedTypeSymbol symbol) => Symbol = symbol;
    }
}