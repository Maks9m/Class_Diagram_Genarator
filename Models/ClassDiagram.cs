namespace ClassDiagramGenerator.Models;

/// <summary>
/// Represents a class in a UML diagram
/// </summary>
public class ClassDiagram
{
    public string ClassName { get; set; } = string.Empty;
    public string? Namespace { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsInterface { get; set; }
    public bool IsStatic { get; set; }
    public string? BaseClass { get; set; }
    public List<string> Interfaces { get; set; } = [];
    public List<ClassMember> Members { get; set; } = [];
    public List<ClassMethod> Methods { get; set; } = [];

    public override string ToString()
    {
        var classType = IsInterface ? "<<interface>>" : IsAbstract ? "<<abstract>>" : "";
        return $"{classType} {ClassName}";
    }
}
