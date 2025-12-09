namespace ClassDiagramGenerator.Models;

/// <summary>
/// Represents a class method in the diagram
/// </summary>
public class ClassMethod
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<MethodParameter> Parameters { get; set; } = [];
    public AccessModifier AccessModifier { get; set; }
    public bool IsStatic { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsVirtual { get; set; }

    public override string ToString()
    {
        var modifiers = GetModifierPrefix();
        var parameters = string.Join(", ", Parameters.Select(p => $"{p.Name}: {p.Type}"));
        return $"{modifiers}{Name}({parameters}): {ReturnType}";
    }

    private string GetModifierPrefix()
    {
        var prefix = AccessModifier switch
        {
            AccessModifier.Public => "+ ",
            AccessModifier.Private => "- ",
            AccessModifier.Protected => "# ",
            AccessModifier.Internal => "~ ",
            _ => ""
        };

        if (IsAbstract)
            prefix += "[abstract] ";
        if (IsStatic)
            prefix += "[static] ";
        if (IsVirtual)
            prefix += "[virtual] ";

        return prefix;
    }
}

public class MethodParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
