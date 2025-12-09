using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using ClassDiagramGenerator.Formatters;
using ClassDiagramGenerator.Models;
using ClassDiagramGenerator.Services;
using Microsoft.Win32;

namespace ClassDiagramGenerator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ReflectionAnalyzer _analyzer = new();
    private readonly ObservableCollection<string> _analyzedTypes = new();
    private readonly Dictionary<string, ClassDiagram> _diagramCache = new();
    private readonly AsciiFormatter _asciiFormatter = new();
    private readonly PlantUmlFormatter _plantUmlFormatter = new();

    public ObservableCollection<string> AnalyzedTypes => _analyzedTypes;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void AnalyzeType_Click(object sender, RoutedEventArgs e)
    {
        var typeName = TypeNameInput.Text.Trim();
        if (string.IsNullOrEmpty(typeName))
        {
            MessageBox.Show("Please enter a type name", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Try to find the type in loaded assemblies
            var type = Type.GetType(typeName);
            if (type == null)
            {
                // Try to find in all loaded assemblies
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType(typeName);
                    if (type != null) break;
                }
            }

            if (type == null)
            {
                MessageBox.Show($"Type '{typeName}' not found", "Type Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AnalyzeAndDisplayType(type);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BrowseAssembly_Click(object sender, RoutedEventArgs e)
    {
        var openDialog = new OpenFileDialog
        {
            Filter = "Assembly Files (*.dll)|*.dll|All Files (*.*)|*.*",
            InitialDirectory = Directory.GetCurrentDirectory()
        };

        if (openDialog.ShowDialog() == true)
        {
            try
            {
                var assembly = Assembly.LoadFrom(openDialog.FileName);
                var diagrams = _analyzer.AnalyzeAssembly(assembly);

                _analyzedTypes.Clear();
                _diagramCache.Clear();

                foreach (var diagram in diagrams)
                {
                    var fullName = $"{diagram.ClassName} ({diagram.Namespace})";
                    _analyzedTypes.Add(fullName);
                    _diagramCache[fullName] = diagram;
                }

                MessageBox.Show($"Loaded {diagrams.Count} types from assembly", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading assembly: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void TypeList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (TypeList.SelectedItem is string selectedItem && _diagramCache.TryGetValue(selectedItem, out var diagram))
        {
            DisplayDiagram(diagram);
        }
    }

    private void AnalyzeAndDisplayType(Type type)
    {
        try
        {
            var diagram = _analyzer.AnalyzeType(type);
            var key = $"{diagram.ClassName} ({diagram.Namespace})";

            if (!_diagramCache.ContainsKey(key))
            {
                _analyzedTypes.Add(key);
            }

            _diagramCache[key] = diagram;
            DisplayDiagram(diagram);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error analyzing type: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DisplayDiagram(ClassDiagram diagram)
    {
        try
        {
            // Display ASCII format
            DiagramOutput.Text = _asciiFormatter.Format(diagram);

            // Display PlantUML format
            PlantUmlOutput.Text = _plantUmlFormatter.Format(diagram);

            // Display information
            var info = GenerateClassInfo(diagram);
            InfoOutput.Text = info;

            // Display relationships
            var diagrams = _diagramCache.Values.ToList();
            var relationships = _analyzer.GenerateRelationshipDiagram(diagrams);
            RelationshipsOutput.Text = relationships;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error displaying diagram: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string GenerateClassInfo(ClassDiagram diagram)
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"Class Name: {diagram.ClassName}");
        info.AppendLine($"Namespace: {diagram.Namespace}");
        info.AppendLine($"Is Abstract: {diagram.IsAbstract}");
        info.AppendLine($"Is Interface: {diagram.IsInterface}");
        info.AppendLine($"Is Static: {diagram.IsStatic}");

        if (!string.IsNullOrEmpty(diagram.BaseClass))
            info.AppendLine($"Extends: {diagram.BaseClass}");

        if (diagram.Interfaces.Any())
            info.AppendLine($"Implements: {string.Join(", ", diagram.Interfaces)}");

        info.AppendLine($"\nMembers ({diagram.Members.Count}):");
        foreach (var member in diagram.Members)
            info.AppendLine($"  {member}");

        info.AppendLine($"\nMethods ({diagram.Methods.Count}):");
        foreach (var method in diagram.Methods)
            info.AppendLine($"  {method}");

        return info.ToString();
    }

    private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
    {
        var format = FormatCombo.SelectedItem?.ToString() ?? "ASCII";
        var text = format == "ASCII" ? DiagramOutput.Text : PlantUmlOutput.Text;

        if (!string.IsNullOrEmpty(text))
        {
            Clipboard.SetText(text);
            MessageBox.Show("Diagram copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void SaveToFile_Click(object sender, RoutedEventArgs e)
    {
        var format = FormatCombo.SelectedItem?.ToString() ?? "ASCII";
        var text = format == "ASCII" ? DiagramOutput.Text : PlantUmlOutput.Text;

        if (string.IsNullOrEmpty(text))
        {
            MessageBox.Show("No diagram to save", "Nothing to Save", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var saveDialog = new SaveFileDialog
        {
            Filter = format == "ASCII" ? "Text Files (*.txt)|*.txt|All Files (*.*)|*.*" : "PlantUML Files (*.puml)|*.puml|All Files (*.*)|*.*",
            DefaultExt = format == "ASCII" ? ".txt" : ".puml",
            FileName = $"diagram_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                File.WriteAllText(saveDialog.FileName, text);
                MessageBox.Show($"Diagram saved to {saveDialog.FileName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        TypeNameInput.Clear();
        DiagramOutput.Clear();
        PlantUmlOutput.Clear();
        InfoOutput.Clear();
        _analyzedTypes.Clear();
        _diagramCache.Clear();
    }
}
