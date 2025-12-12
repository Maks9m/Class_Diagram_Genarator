namespace ClassDiagramGenerator.Models;

/// <summary>
/// Represents a class member (field or property)
/// </summary>
public class ClassMember
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public AccessModifier AccessModifier { get; set; }
    public bool IsStatic { get; set; }
    public bool IsReadOnly { get; set; }

    public override string ToString()
    {
        var modifiers = GetModifierPrefix();
        return $"{modifiers}{Name}: {Type}";
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

        if (IsStatic)
            prefix += "[static] ";
        if (IsReadOnly)
            prefix += "[readonly] ";

        return prefix;
    }
}
