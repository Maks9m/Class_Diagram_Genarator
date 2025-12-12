using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        TypeNameInput.Clear();
        DiagramOutput.Clear();
        PlantUmlOutput.Clear();
        InfoOutput.Clear();
        _analyzedTypes.Clear();
        _diagramCache.Clear();
    }

    private async void PreviewPuml_Click(object sender, RoutedEventArgs e)
    {
        var pumlCode = PumlInput.Text.Trim();
        if (string.IsNullOrEmpty(pumlCode))
        {
            MessageBox.Show("Please enter PlantUML code", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Sanitize PlantUML code - fix backtick generic notation
            var sanitized = SanitizePlantUml(pumlCode);
            var encoded = EncodePlantUml(sanitized);
            var url = $"https://www.plantuml.com/plantuml/png/{encoded}";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            var imageBytes = await client.GetByteArrayAsync(url);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(imageBytes);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            PumlPreviewImage.Source = bitmap;
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"HTTP Error: {ex.Message}\n\nThe PlantUML code may be too complex or contain unsupported syntax.", "Preview Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating preview: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Sanitizes PlantUML code to fix common issues
    /// </summary>
    private static string SanitizePlantUml(string puml)
    {
        // Remove backtick generic notation (e.g., IEnumerable`1 -> IEnumerable)
        return System.Text.RegularExpressions.Regex.Replace(puml, @"`\d+", "");
    }

    private void UseGenerated_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PlantUmlOutput.Text))
        {
            PumlInput.Text = PlantUmlOutput.Text;
        }
        else
        {
            MessageBox.Show("No generated PlantUML code available. Analyze a type first.", "No Code", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private static string EncodePlantUml(string text)
    {
        var compressed = Deflate(Encoding.UTF8.GetBytes(text));
        return Encode64(compressed);
    }

    private static byte[] Deflate(byte[] data)
    {
        using var output = new MemoryStream();
        using (var deflate = new DeflateStream(output, CompressionLevel.Optimal))
        {
            deflate.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    private static string Encode64(byte[] data)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < data.Length; i += 3)
        {
            if (i + 2 == data.Length)
            {
                sb.Append(Append3Bytes(data[i], data[i + 1], 0));
            }
            else if (i + 1 == data.Length)
            {
                sb.Append(Append3Bytes(data[i], 0, 0));
            }
            else
            {
                sb.Append(Append3Bytes(data[i], data[i + 1], data[i + 2]));
            }
        }
        return sb.ToString();
    }

    private static string Append3Bytes(byte b1, byte b2, byte b3)
    {
        int c1 = b1 >> 2;
        int c2 = ((b1 & 0x3) << 4) | (b2 >> 4);
        int c3 = ((b2 & 0xF) << 2) | (b3 >> 6);
        int c4 = b3 & 0x3F;
        return $"{Encode6Bit(c1)}{Encode6Bit(c2)}{Encode6Bit(c3)}{Encode6Bit(c4)}";
    }

    private static char Encode6Bit(int b)
    {
        if (b < 10) return (char)(48 + b);
        b -= 10;
        if (b < 26) return (char)(65 + b);
        b -= 26;
        if (b < 26) return (char)(97 + b);
        b -= 26;
        if (b == 0) return '-';
        return '_';
    }
}
