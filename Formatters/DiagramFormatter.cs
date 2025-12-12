using ClassDiagramGenerator.Models;
using System.Text;

namespace ClassDiagramGenerator.Formatters;

/// <summary>
/// Base interface for diagram formatters
/// </summary>
public interface IDiagramFormatter
{
    string Format(ClassDiagram diagram);
    string FormatMultiple(List<ClassDiagram> diagrams);
}

/// <summary>
/// Formats diagrams as ASCII text
/// </summary>
public class AsciiFormatter : IDiagramFormatter
{
    private const int BoxWidth = 50;

    public string Format(ClassDiagram diagram)
    {
        var sb = new StringBuilder();
        DrawBox(sb, diagram);
        return sb.ToString();
    }

    public string FormatMultiple(List<ClassDiagram> diagrams)
    {
        var sb = new StringBuilder();
        
        for (int i = 0; i < diagrams.Count; i++)
        {
            sb.Append(Format(diagrams[i]));
            if (i < diagrams.Count - 1)
                sb.AppendLine("\n");
        }

        return sb.ToString();
    }

    private void DrawBox(StringBuilder sb, ClassDiagram diagram)
    {
        var classType = diagram.IsInterface ? "<<interface>>" :
                       diagram.IsAbstract ? "<<abstract>>" : "";

        // Top border: ┌────────────────────┐
        sb.Append('┌');
        sb.Append(new string('─', BoxWidth - 2));
        sb.AppendLine("┐");
        
        if (!string.IsNullOrEmpty(classType))
            CenterAndAppend(sb, classType);
        
        CenterAndAppend(sb, diagram.ClassName);
        
        // Separator: ├────────────────────┤
        sb.Append('├');
        sb.Append(new string('─', BoxWidth - 2));
        sb.AppendLine("┤");

        if (!string.IsNullOrEmpty(diagram.BaseClass))
            AppendLine(sb, $"extends: {diagram.BaseClass}");

        if (diagram.Interfaces.Any())
            AppendLine(sb, $"implements: {string.Join(", ", diagram.Interfaces)}");

        if (diagram.Members.Any())
        {
            sb.Append('├');
            sb.Append(new string('─', BoxWidth - 2));
            sb.AppendLine("┤");
            foreach (var member in diagram.Members)
            {
                AppendLine(sb, member.ToString());
            }
        }

        if (diagram.Methods.Any())
        {
            sb.Append('├');
            sb.Append(new string('─', BoxWidth - 2));
            sb.AppendLine("┤");
            foreach (var method in diagram.Methods)
            {
                AppendLine(sb, method.ToString());
            }
        }

        // Bottom border: └────────────────────┘
        sb.Append('└');
        sb.Append(new string('─', BoxWidth - 2));
        sb.AppendLine("┘");
    }

    private void CenterAndAppend(StringBuilder sb, string text)
    {
        var innerWidth = BoxWidth - 2; // Space between the two │ characters
        var safeLengthText = text.Length > innerWidth ? text.Substring(0, Math.Max(1, innerWidth - 3)) + "..." : text;
        var totalPadding = innerWidth - safeLengthText.Length;
        var leftPadding = totalPadding / 2;
        var rightPadding = totalPadding - leftPadding;
        
        sb.Append('│');
        sb.Append(new string(' ', leftPadding));
        sb.Append(safeLengthText);
        sb.Append(new string(' ', rightPadding));
        sb.AppendLine("│");
    }

    private void AppendLine(StringBuilder sb, string text)
    {
        var maxLength = BoxWidth - 4; // Account for "│ " and " │"
        var truncated = text.Length > maxLength ? text.Substring(0, Math.Max(1, maxLength - 3)) + "..." : text;
        var padding = Math.Max(0, BoxWidth - truncated.Length - 3);
        
        sb.Append("│ ");
        sb.Append(truncated);
        sb.Append(new string(' ', padding));
        sb.AppendLine("│");
    }
}

/// <summary>
/// Formats diagrams in PlantUML format
/// </summary>
public class PlantUmlFormatter : IDiagramFormatter
{
    public string Format(ClassDiagram diagram)
    {
        var sb = new StringBuilder();
        sb.AppendLine("@startuml");
        sb.AppendLine("class " + diagram.ClassName + " {");

        if (diagram.Members.Any())
        {
            foreach (var member in diagram.Members)
            {
                sb.AppendLine("  " + member.ToString());
            }
        }

        if (diagram.Methods.Any())
        {
            foreach (var method in diagram.Methods)
            {
                sb.AppendLine("  " + method.ToString());
            }
        }

        sb.AppendLine("}");

        if (!string.IsNullOrEmpty(diagram.BaseClass))
        {
            sb.AppendLine($"{diagram.ClassName} --|> {diagram.BaseClass}");
        }

        foreach (var iface in diagram.Interfaces)
        {
            sb.AppendLine($"{diagram.ClassName} ..|> {iface}");
        }

        sb.AppendLine("@enduml");
        return sb.ToString();
    }

    public string FormatMultiple(List<ClassDiagram> diagrams)
    {
        var sb = new StringBuilder();
        sb.AppendLine("@startuml");

        foreach (var diagram in diagrams)
        {
            sb.AppendLine($"class {diagram.ClassName} {{");

            if (diagram.Members.Any())
            {
                foreach (var member in diagram.Members)
                {
                    sb.AppendLine("  " + member.ToString());
                }
            }

            if (diagram.Methods.Any())
            {
                foreach (var method in diagram.Methods)
                {
                    sb.AppendLine("  " + method.ToString());
                }
            }

            sb.AppendLine("}");

            if (!string.IsNullOrEmpty(diagram.BaseClass))
            {
                sb.AppendLine($"{diagram.ClassName} --|> {diagram.BaseClass}");
            }

            foreach (var iface in diagram.Interfaces)
            {
                sb.AppendLine($"{diagram.ClassName} ..|> {iface}");
            }
        }

        sb.AppendLine("@enduml");
        return sb.ToString();
    }
}
