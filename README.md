# Class Diagram Generator

A C# console application that generates UML class diagrams from .NET types using reflection and outputs them in multiple formats (ASCII and PlantUML).

## Features

✨ **Reflection-Based Analysis**
- Automatically analyze any .NET type and extract its structure
- Analyze entire assemblies at once
- Extract members, methods, properties, and their access modifiers

📊 **Multiple Output Formats**
- **ASCII Format**: Beautiful console output with box drawing characters
- **PlantUML Format**: Generate diagrams compatible with PlantUML

🎯 **UML Support**
- Access modifiers (Public, Private, Protected, Internal)
- Class inheritance (base classes)
- Interface implementation
- Abstract and static members
- Method parameters and return types
- Properties and fields with metadata

## Project Structure

```
ClassDiagramGenerator/
├── Models/
│   ├── AccessModifier.cs      # Enum for UML access modifiers
│   ├── ClassDiagram.cs        # Main diagram model
│   ├── ClassMember.cs         # Class fields and properties
│   └── ClassMethod.cs         # Class methods and parameters
├── Services/
│   └── ReflectionAnalyzer.cs  # Reflection-based type analyzer
├── Formatters/
│   └── DiagramFormatter.cs    # ASCII and PlantUML formatters
├── Program.cs                 # Main entry point with examples
└── ClassDiagramGenerator.csproj
```

## Usage

### Build the Project

```bash
dotnet build
```

### Run the Application

```bash
dotnet run
```

### Programmatic Usage

```csharp
using ClassDiagramGenerator.Services;
using ClassDiagramGenerator.Formatters;

// Create analyzer
var analyzer = new ReflectionAnalyzer();

// Analyze a type
var diagram = analyzer.AnalyzeType(typeof(MyClass));

// Format as ASCII
var asciiFormatter = new AsciiFormatter();
Console.WriteLine(asciiFormatter.Format(diagram));

// Format as PlantUML
var plantumlFormatter = new PlantUmlFormatter();
Console.WriteLine(plantumlFormatter.Format(diagram));
```

### Analyze an Assembly

```csharp
var assembly = Assembly.Load("MyAssembly");
var diagrams = analyzer.AnalyzeAssembly(assembly);

var formatter = new AsciiFormatter();
Console.WriteLine(formatter.FormatMultiple(diagrams));
```

## Example Output

### ASCII Format
```
┌──────────────────────────────────────────────────┐
│                   Employee                       │
├──────────────────────────────────────────────────┤
│ - id: int                                         │
│ - name: string                                    │
│ - salary: decimal                                 │
│ + department: string                              │
├──────────────────────────────────────────────────┤
│ + GetSalary(): decimal                            │
│ + SetSalary(newSalary: decimal): void            │
│ + [virtual] ToString(): string                    │
└──────────────────────────────────────────────────┘
```

### PlantUML Format
```
@startuml
class Employee {
  - id: int
  - name: string
  - salary: decimal
  + department: string
  + GetSalary(): decimal
  + SetSalary(newSalary: decimal): void
  + [virtual] ToString(): string
}
@enduml
```

## Access Modifiers

- `+` : Public
- `-` : Private
- `#` : Protected
- `~` : Internal

## Special Annotations

- `[abstract]` : Abstract member
- `[static]` : Static member
- `[virtual]` : Virtual member
- `[readonly]` : Read-only field

## Requirements

- .NET 8.0 or later
- C# 11 or later

## Example Classes

The application includes examples that analyze:
- Local project classes (ClassDiagram, ClassMember, ClassMethod)
- System.Collections types (List, Dictionary, HashSet)
- Custom created diagrams (Employee, IPerson)

## Extensibility

You can extend the application by:

1. **Creating Custom Formatters**: Implement `IDiagramFormatter` interface
   ```csharp
   public class CustomFormatter : IDiagramFormatter
   {
       public string Format(ClassDiagram diagram) { ... }
       public string FormatMultiple(List<ClassDiagram> diagrams) { ... }
   }
   ```

2. **Adding More Analysis**: Extend `ReflectionAnalyzer` to extract additional type information

3. **Supporting More Output Formats**: Add formatters for Mermaid, SVG, JSON, etc.

## Future Enhancements

- [ ] SVG diagram generation
- [ ] JSON output format
- [ ] Mermaid diagram format
- [ ] Relationship visualization (aggregation, composition)
- [ ] Generic type support improvements
- [ ] C# 8+ nullable reference types support
- [ ] Custom type filter/include rules

## License

This project is open source and available under the MIT License.

## Author

Created as an educational tool for understanding .NET reflection and UML diagrams.
