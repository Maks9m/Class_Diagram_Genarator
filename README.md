# Class Diagram Generator

A WPF desktop application that generates UML class diagrams from .NET types using reflection and outputs them in multiple formats (ASCII and PlantUML).

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![WPF](https://img.shields.io/badge/WPF-Desktop-green)
![License](https://img.shields.io/badge/License-MIT-yellow)

## Features

🖥️ **Modern WPF Desktop Application**
- Clean, intuitive user interface
- Tabbed view for different output formats
- Type browser panel for analyzed assemblies
- Copy to clipboard functionality

✨ **Reflection-Based Analysis**
- Analyze any .NET type and extract its structure
- Load and analyze entire DLL assemblies
- Extract members, methods, properties, and their access modifiers

📊 **Multiple Output Formats**
- **ASCII Format**: Beautiful text output with box drawing characters
- **PlantUML Format**: Generate diagrams compatible with PlantUML

🔍 **Live PlantUML Preview**
- Built-in PlantUML code editor with syntax highlighting
- Real-time diagram preview using PlantUML online server
- Use generated code or write your own PlantUML

🎯 **Full UML Support**
- Access modifiers (Public, Private, Protected, Internal)
- Class inheritance (base classes)
- Interface implementation
- Abstract and static members
- Method parameters and return types
- Properties and fields with metadata

## Screenshots

The application features four main tabs:
- **Diagram View**: ASCII art representation of classes
- **PlantUML Code**: Generated PlantUML source code
- **Information**: Detailed class metadata
- **PlantUML Preview**: Live preview with code editor

## Project Structure

```
ClassDiagramGenerator/
├── Models/
│   ├── AccessModifier.cs      # Enum for UML access modifiers
│   ├── ClassDiagram.cs        # Main diagram model
│   ├── ClassMember.cs         # Class fields and properties
│   ├── ClassMethod.cs         # Class methods and parameters
│   └── ClassRelationship.cs   # Class relationship types
├── Services/
│   └── ReflectionAnalyzer.cs  # Reflection-based type analyzer
├── Formatters/
│   └── DiagramFormatter.cs    # ASCII and PlantUML formatters
├── App.xaml                   # WPF Application definition
├── App.xaml.cs
├── MainWindow.xaml            # Main window UI
├── MainWindow.xaml.cs         # Main window logic
└── ClassDiagramGenerator.csproj
```

### Project Classes Diagram
```
@startuml ClassDiagramGenerator

skinparam classAttributeIconSize 0
skinparam linetype ortho

package "ClassDiagramGenerator.Models" {
    enum AccessModifier {
        Public
        Private
        Protected
        Internal
    }

    enum RelationshipType {
        Inherits
        Implements
        Uses
        DependsOn
    }

    class ClassDiagram {
        +ClassName: String
        +Namespace: String?
        +IsAbstract: bool
        +IsInterface: bool
        +IsStatic: bool
        +BaseClass: String?
        +Interfaces: List<String>
        +Members: List<ClassMember>
        +Methods: List<ClassMethod>
        +ToString(): String
    }

    class ClassMember {
        +Name: String
        +Type: String
        +AccessModifier: AccessModifier
        +IsStatic: bool
        +IsReadOnly: bool
        +ToString(): String
        -GetModifierPrefix(): String
    }

    class ClassMethod {
        +Name: String
        +ReturnType: String
        +Parameters: List<MethodParameter>
        +AccessModifier: AccessModifier
        +IsStatic: bool
        +IsAbstract: bool
        +IsVirtual: bool
        +ToString(): String
        -GetModifierPrefix(): String
    }

    class MethodParameter {
        +Name: String
        +Type: String
    }

    class ClassRelationship {
        +SourceClass: String
        +TargetClass: String
        +Type: RelationshipType
        +SourceNamespace: String?
        +TargetNamespace: String?
    }
}

package "ClassDiagramGenerator.Services" {
    class ReflectionAnalyzer {
        +AnalyzeType(type: Type): ClassDiagram
        +AnalyzeAssembly(assembly: Assembly): List<ClassDiagram>
        -GetBaseClassName(type: Type): String?
        -GetInterfaceNames(type: Type): List<String>
        -ExtractMembers(type: Type): List<ClassMember>
        -ExtractMethods(type: Type): List<ClassMethod>
        -GetTypeName(type: Type): String
        -GetAccessModifier(field: FieldInfo): AccessModifier
        -GetAccessModifier(property: PropertyInfo): AccessModifier
        -GetAccessModifier(method: MethodInfo): AccessModifier
    }
}

package "ClassDiagramGenerator.Formatters" {
    interface IDiagramFormatter {
        +Format(diagram: ClassDiagram): String
        +FormatMultiple(diagrams: List<ClassDiagram>): String
    }

    class AsciiFormatter {
        -BoxWidth: int
        +Format(diagram: ClassDiagram): String
        +FormatMultiple(diagrams: List<ClassDiagram>): String
        -DrawBox(sb: StringBuilder, diagram: ClassDiagram): void
        -CenterAndAppend(sb: StringBuilder, text: String): void
        -AppendLine(sb: StringBuilder, text: String): void
    }

    class PlantUmlFormatter {
        +Format(diagram: ClassDiagram): String
        +FormatMultiple(diagrams: List<ClassDiagram>): String
    }
}

package "ClassDiagramGenerator" {
    class MainWindow {
        -_analyzer: ReflectionAnalyzer
        -_analyzedTypes: ObservableCollection<String>
        -_diagramCache: Dictionary<String, ClassDiagram>
        -_asciiFormatter: AsciiFormatter
        -_plantUmlFormatter: PlantUmlFormatter
        +AnalyzedTypes: ObservableCollection<String>
        -AnalyzeType_Click(): void
        -BrowseAssembly_Click(): void
        -DisplayDiagram(diagram: ClassDiagram): void
        -PreviewPuml_Click(): void
    }
}

' Relationships
ClassDiagram "1" *-- "*" ClassMember : contains
ClassDiagram "1" *-- "*" ClassMethod : contains
ClassMethod "1" *-- "*" MethodParameter : contains
ClassMember --> AccessModifier : uses
ClassMethod --> AccessModifier : uses
ClassRelationship --> RelationshipType : uses

AsciiFormatter ..|> IDiagramFormatter
PlantUmlFormatter ..|> IDiagramFormatter

ReflectionAnalyzer ..> ClassDiagram : creates
ReflectionAnalyzer ..> ClassMember : creates
ReflectionAnalyzer ..> ClassMethod : creates

MainWindow --> ReflectionAnalyzer : uses
MainWindow --> AsciiFormatter : uses
MainWindow --> PlantUmlFormatter : uses
MainWindow --> ClassDiagram : displays
@enduml
```

## Usage

### Build the Project

```bash
dotnet build ClassDiagramGenerator.csproj
```
If you have build error (used bu another proccess):
```bash
Stop-Process -Name "ClassDiagramGenerator" -Force -ErrorAction SilentlyContinue;
```

### Run the Application

```bash
dotnet run
```

Or run the executable directly:
```bash
./bin/Debug/net8.0-windows/ClassDiagramGenerator.exe
```

### Using the Application

1. **Analyze a Type**: Enter a fully-qualified type name (e.g., `System.String`) and click "Analyze Type"
2. **Browse Assembly**: Click "Browse Assembly" to load a DLL file and analyze all its public types
3. **View Results**: Use the tabs to switch between ASCII diagram, PlantUML code, and class information
4. **PlantUML Preview**: Go to the "PlantUML Preview" tab to write custom PlantUML code and see a live preview
5. **Copy Output**: Select output format and click "Copy to Clipboard"

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
│ - id: int                                        │
│ - name: string                                   │
│ - salary: decimal                                │
│ + department: string                             │
├──────────────────────────────────────────────────┤
│ + GetSalary(): decimal                           │
│ + SetSalary(newSalary: decimal): void            │
│ + [virtual] ToString(): string                   │
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

- .NET 8.0 SDK or later
- Windows (WPF application)

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

- [ ] Visual diagram canvas with relationship arrows
- [ ] SVG/PNG export functionality
- [ ] Mermaid diagram format support
- [ ] JSON output format
- [ ] Assembly comparison tool
- [ ] Save/Load project sessions
- [ ] Dark theme support

## License

This project is open source and available under the MIT License.

## Author

Created as an educational tool for understanding .NET reflection and UML diagrams.
