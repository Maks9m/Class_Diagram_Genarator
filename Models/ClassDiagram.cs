namespace ClassDiagramGenerator.Models;

/// <summary>
/// Represents a class in a UML diagram
/// </summary>
public class ClassDiagram
{
    /// <summary>
    /// Maximum number of members to display (prioritized by access modifier)
    /// </summary>
    public const int MaxDisplayMembers = 8;

    /// <summary>
    /// Maximum number of methods to display (prioritized by access modifier)
    /// </summary>
    public const int MaxDisplayMethods = 10;

    public string ClassName { get; set; } = string.Empty;
    public string? Namespace { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsInterface { get; set; }
    public bool IsStatic { get; set; }
    public string? BaseClass { get; set; }
    public List<string> Interfaces { get; set; } = [];
    public List<ClassMember> Members { get; set; } = [];
    public List<ClassMethod> Methods { get; set; } = [];

    /// <summary>
    /// Gets members sorted by priority (Public first, then Private, Protected, Internal)
    /// Limited to MaxDisplayMembers
    /// </summary>
    public (List<ClassMember> displayed, int hiddenCount) GetDisplayMembers(int? limit = null)
    {
        var maxItems = limit ?? MaxDisplayMembers;
        var sorted = Members
            .OrderBy(m => GetAccessPriority(m.AccessModifier))
            .ToList();

        if (sorted.Count <= maxItems)
            return (sorted, 0);

        return (sorted.Take(maxItems).ToList(), sorted.Count - maxItems);
    }

    /// <summary>
    /// Gets methods sorted by priority (Public first, then Private, Protected, Internal)
    /// Limited to MaxDisplayMethods
    /// </summary>
    public (List<ClassMethod> displayed, int hiddenCount) GetDisplayMethods(int? limit = null)
    {
        var maxItems = limit ?? MaxDisplayMethods;
        var sorted = Methods
            .OrderBy(m => GetAccessPriority(m.AccessModifier))
            .ToList();

        if (sorted.Count <= maxItems)
            return (sorted, 0);

        return (sorted.Take(maxItems).ToList(), sorted.Count - maxItems);
    }

    /// <summary>
    /// Returns priority order: Public (0), Private (1), Protected (2), Internal (3)
    /// </summary>
    private static int GetAccessPriority(AccessModifier modifier) => modifier switch
    {
        AccessModifier.Public => 0,
        AccessModifier.Private => 1,
        AccessModifier.Protected => 2,
        AccessModifier.Internal => 3,
        _ => 4
    };

    public override string ToString()
    {
        var classType = IsInterface ? "<<interface>>" : IsAbstract ? "<<abstract>>" : "";
        return $"{classType} {ClassName}";
    }
}
