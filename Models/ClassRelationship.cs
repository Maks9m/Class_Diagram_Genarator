namespace ClassDiagramGenerator.Models;

/// <summary>
/// Represents relationships between classes
/// </summary>
public class ClassRelationship
{
    public string SourceClass { get; set; } = string.Empty;
    public string TargetClass { get; set; } = string.Empty;
    public RelationshipType Type { get; set; }
    public string? SourceNamespace { get; set; }
    public string? TargetNamespace { get; set; }
}

/// <summary>
/// Types of relationships between classes
/// </summary>
public enum RelationshipType
{
    /// <summary>
    /// Class A extends Class B (inheritance)
    /// </summary>
    Inherits,

    /// <summary>
    /// Class A implements Interface B
    /// </summary>
    Implements,

    /// <summary>
    /// Class A uses Class B as a property/field
    /// </summary>
    Uses,

    /// <summary>
    /// Class A depends on Class B
    /// </summary>
    DependsOn
}
