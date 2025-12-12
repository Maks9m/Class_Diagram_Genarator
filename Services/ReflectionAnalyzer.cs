using System.Reflection;
using ClassDiagramGenerator.Models;

namespace ClassDiagramGenerator.Services;

/// <summary>
/// Analyzes .NET types using reflection and generates class diagrams
/// </summary>
public class ReflectionAnalyzer
{
    public ClassDiagram AnalyzeType(Type type)
    {
        var diagram = new ClassDiagram
        {
            ClassName = type.Name,
            Namespace = type.Namespace,
            IsAbstract = type.IsAbstract,
            IsInterface = type.IsInterface,
            IsStatic = type.IsAbstract && type.IsSealed,
            BaseClass = GetBaseClassName(type),
            Interfaces = GetInterfaceNames(type),
            Members = ExtractMembers(type),
            Methods = ExtractMethods(type)
        };

        return diagram;
    }

    public List<ClassDiagram> AnalyzeAssembly(Assembly assembly)
    {
        var diagrams = new List<ClassDiagram>();
        var publicTypes = assembly.GetExportedTypes();

        foreach (var type in publicTypes)
        {
            if (!type.IsNestedPrivate && !type.IsNestedAssembly)
            {
                diagrams.Add(AnalyzeType(type));
            }
        }

        return diagrams;
    }

    private string? GetBaseClassName(Type type)
    {
        if (type.BaseType is null || type.BaseType == typeof(object))
            return null;

        return type.BaseType.Name;
    }

    private List<string> GetInterfaceNames(Type type)
    {
        return type.GetInterfaces()
            .Select(i => i.Name)
            .ToList();
    }

    private List<ClassMember> ExtractMembers(Type type)
    {
        var members = new List<ClassMember>();

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        foreach (var field in fields)
        {
            if (field.IsSpecialName || field.Name.StartsWith("<"))
                continue;

            members.Add(new ClassMember
            {
                Name = field.Name,
                Type = GetTypeName(field.FieldType),
                AccessModifier = GetAccessModifier(field),
                IsStatic = field.IsStatic,
                IsReadOnly = field.IsInitOnly
            });
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        foreach (var prop in properties)
        {
            if (prop.IsSpecialName)
                continue;

            members.Add(new ClassMember
            {
                Name = prop.Name,
                Type = GetTypeName(prop.PropertyType),
                AccessModifier = GetAccessModifier(prop),
                IsStatic = (prop.GetMethod?.IsStatic ?? false) || (prop.SetMethod?.IsStatic ?? false),
            });
        }

        return members;
    }

    private List<ClassMethod> ExtractMethods(Type type)
    {
        var methods = new List<ClassMethod>();
        var methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

        foreach (var method in methodInfos)
        {
            if (method.IsSpecialName || method.Name.StartsWith("<"))
                continue;

            var parameters = method.GetParameters()
                .Select(p => new MethodParameter { Name = p.Name ?? "param", Type = GetTypeName(p.ParameterType) })
                .ToList();

            methods.Add(new ClassMethod
            {
                Name = method.Name,
                ReturnType = GetTypeName(method.ReturnType),
                Parameters = parameters,
                AccessModifier = GetAccessModifier(method),
                IsStatic = method.IsStatic,
                IsAbstract = method.IsAbstract,
                IsVirtual = method.IsVirtual && !method.IsFinal
            });
        }

        return methods;
    }

    private AccessModifier GetAccessModifier(FieldInfo field)
    {
        return field.IsPublic ? AccessModifier.Public :
               field.IsPrivate ? AccessModifier.Private :
               field.IsFamily ? AccessModifier.Protected :
               AccessModifier.Internal;
    }

    private AccessModifier GetAccessModifier(PropertyInfo property)
    {
        var getterAccessibility = property.GetMethod?.IsPublic ?? false ? AccessModifier.Public :
                                  property.GetMethod?.IsPrivate ?? false ? AccessModifier.Private :
                                  property.GetMethod?.IsFamily ?? false ? AccessModifier.Protected :
                                  AccessModifier.Internal;

        return getterAccessibility;
    }

    private AccessModifier GetAccessModifier(MethodInfo method)
    {
        return method.IsPublic ? AccessModifier.Public :
               method.IsPrivate ? AccessModifier.Private :
               method.IsFamily ? AccessModifier.Protected :
               AccessModifier.Internal;
    }

    private string GetTypeName(Type type)
    {
        // Handle nullable reference types
        if (Nullable.GetUnderlyingType(type) is Type underlyingType)
        {
            return GetTypeName(underlyingType) + "?";
        }

        // Handle generic types
        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            try
            {
                var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetTypeName));
                var backtickIndex = type.Name.IndexOf('`');
                var baseName = backtickIndex >= 0 ? type.Name.Substring(0, backtickIndex) : type.Name;
                return $"{baseName}<{genericArgs}>";
            }
            catch
            {
                return type.Name;
            }
        }

        return type.Name;
    }
}
