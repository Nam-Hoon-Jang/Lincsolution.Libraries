using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Lincsolution.Core.SourceGenerator
{
    [Generator(LanguageNames.CSharp)]
    public sealed class DependencyPropertyGenerator : IIncrementalGenerator
    {
        private const string AttributeMetadataName =
            "Lincsolution.UI.Base.Common.DependencyPropertyAttribute";

        public void Initialize(
            IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<PropertyModel> properties =
                context.SyntaxProvider
                    .ForAttributeWithMetadataName(
                        AttributeMetadataName,
                        IsTargetProperty,
                        CreateModel)
                    .Where(IsValidModel);

            context.RegisterSourceOutput(
                properties,
                Generate);
        }

        private static bool IsTargetProperty(
            SyntaxNode node,
            CancellationToken cancellationToken)
        {
            return node is PropertyDeclarationSyntax;
        }

        private static bool IsValidModel(PropertyModel model)
        {
            return model != null;
        }

        private static PropertyModel CreateModel(
            GeneratorAttributeSyntaxContext context,
            CancellationToken cancellationToken)
        {
            IPropertySymbol property =
                context.TargetSymbol as IPropertySymbol;

            if(property == null)
                return null;

            if(property.ContainingType == null)
                return null;

            INamedTypeSymbol schema = property.ContainingType;

            if(schema.TypeKind != TypeKind.Interface ||
               schema.ContainingType == null ||
               schema.ContainingType.TypeKind != TypeKind.Class)
            {
                return null;
            }

            INamedTypeSymbol owner = schema.ContainingType;

            AttributeData attribute = context.Attributes[0];

            bool hasAttributeDefaultValue = false;
            TypedConstant attributeDefaultValue = default(TypedConstant);

            long options = 0;
            string changedCallback = null;

            foreach(KeyValuePair<string, TypedConstant> argument
                in attribute.NamedArguments)
            {
                switch(argument.Key)
                {
                    case "DefaultValue":
                        hasAttributeDefaultValue = true;
                        attributeDefaultValue = argument.Value;
                        break;

                    case "Options":
                        if(argument.Value.Value != null)
                        {
                            options = Convert.ToInt64(
                                argument.Value.Value,
                                CultureInfo.InvariantCulture);
                        }
                        break;

                    case "ChangedCallback":
                        changedCallback =
                            argument.Value.Value as string;
                        break;
                }
            }

            string defaultMemberName =
                FindConventionDefaultMember(
                    context.SemanticModel.Compilation,
                    owner,
                    property);

            return new PropertyModel(
                owner,
                property,
                hasAttributeDefaultValue,
                attributeDefaultValue,
                options,
                changedCallback,
                defaultMemberName);
        }

        /// <summary>
        /// {PropertyName}Default라는 static field 또는
        /// static property를 검색한다.
        /// </summary>
        private static string FindConventionDefaultMember(
            Compilation compilation,
            INamedTypeSymbol owner,
            IPropertySymbol property)
        {
            string memberName = property.Name + "Default";

            ImmutableArray<ISymbol> members =
                owner.GetMembers(memberName);

            foreach(ISymbol member in members)
            {
                ITypeSymbol memberType = null;
                bool isStatic = false;

                IFieldSymbol field = member as IFieldSymbol;

                if(field != null)
                {
                    memberType = field.Type;
                    isStatic = field.IsStatic;
                }
                else
                {
                    IPropertySymbol defaultProperty =
                        member as IPropertySymbol;

                    if(defaultProperty != null)
                    {
                        memberType = defaultProperty.Type;

                        isStatic =
                            defaultProperty.IsStatic &&
                            defaultProperty.GetMethod != null;
                    }
                }

                if(!isStatic || memberType == null)
                    continue;

                Conversion conversion =
                    compilation.ClassifyConversion(
                        memberType,
                        property.Type);

                if(conversion.IsImplicit)
                    return memberName;
            }

            return null;
        }

        private static void Generate(
            SourceProductionContext context,
            PropertyModel model)
        {
            IPropertySymbol property = model.Property;
            INamedTypeSymbol owner = model.Owner;

            StringBuilder source = new StringBuilder();

            source.AppendLine("// <auto-generated />");
            source.AppendLine();

            string namespaceName = null;

            if(!owner.ContainingNamespace.IsGlobalNamespace)
            {
                namespaceName =
                    owner.ContainingNamespace.ToDisplayString();
            }

            string indent = string.Empty;

            if(namespaceName != null)
            {
                source.Append("namespace ");
                source.Append(namespaceName);
                source.AppendLine();
                source.AppendLine("{");

                indent = "    ";
            }

            List<INamedTypeSymbol> containingTypes =
                GetContainingTypes(owner);

            for(int i = 0; i < containingTypes.Count; i++)
            {
                INamedTypeSymbol type = containingTypes[i];

                source.Append(indent);
                source.Append(
                    GetAccessibility(
                        type.DeclaredAccessibility));

                source.Append("partial class ");
                source.Append(
                    GetTypeDeclarationName(type));

                source.AppendLine();

                source.Append(indent);
                source.AppendLine("{");

                indent += "    ";
            }

            GenerateDependencyProperty(
                source,
                indent,
                model);

            source.AppendLine();

            GeneratePropertyImplementation(
                source,
                indent,
                model);

            for(int i = containingTypes.Count - 1;
                 i >= 0;
                 i--)
            {
                indent =
                    indent.Substring(
                        0,
                        indent.Length - 4);

                source.Append(indent);
                source.AppendLine("}");
            }

            if(namespaceName != null)
                source.AppendLine("}");

            string hintName =
                CreateHintName(
                    owner,
                    property);

            context.AddSource(
                hintName,
                SourceText.From(
                    source.ToString(),
                    Encoding.UTF8));
        }

        private static void GenerateDependencyProperty(
            StringBuilder source,
            string indent,
            PropertyModel model)
        {
            IPropertySymbol property = model.Property;

            string propertyType =
                property.Type.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat);

            string ownerType =
                model.Owner.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat);

            source.Append(indent).Append("/// <summary>KO: ").Append(property.Name)
                .Append(" 종속성 속성 식별자. EN: Identifies the ").Append(property.Name)
                .AppendLine(" dependency property.</summary>");
            source.Append(indent);
            source.Append(
                "public static readonly global::System.Windows.DependencyProperty ");
            source.Append(property.Name);
            source.AppendLine("Property =");

            source.Append(indent);
            source.AppendLine(
                "    global::System.Windows.DependencyProperty.Register(");

            source.Append(indent);
            source.Append("        nameof(");
            source.Append(property.Name);
            source.AppendLine("),");

            source.Append(indent);
            source.Append("        typeof(");
            source.Append(propertyType);
            source.AppendLine("),");

            source.Append(indent);
            source.Append("        typeof(");
            source.Append(ownerType);
            source.Append(")");

            string metadata =
                CreateMetadataExpression(model);

            if(metadata == null)
            {
                source.AppendLine(");");
            }
            else
            {
                source.AppendLine(",");

                source.Append(indent);
                source.Append("        ");
                source.Append(metadata);
                source.AppendLine(");");
            }
        }

        private static string CreateMetadataExpression(
            PropertyModel model)
        {
            bool hasDefault =
                model.HasAttributeDefaultValue ||
                model.DefaultMemberName != null;

            bool hasOptions =
                model.Options != 0;

            bool hasCallback =
                !string.IsNullOrEmpty(
                    model.ChangedCallback);

            /*
             * 아무 설정도 없다면
             *
             * DependencyProperty.Register(
             *     name,
             *     type,
             *     owner);
             *
             * 로 생성한다.
             */
            if(!hasDefault &&
                !hasOptions &&
                !hasCallback)
            {
                return null;
            }

            string defaultValue;

            /*
             * 우선순위:
             *
             * 1. Attribute DefaultValue
             * 2. PropertyNameDefault
             * 3. default(T)
             */
            if(model.HasAttributeDefaultValue)
            {
                defaultValue =
                    FormatAttributeDefaultValue(
                        model.AttributeDefaultValue,
                        model.Property.Type);
            }
            else if(model.DefaultMemberName != null)
            {
                defaultValue =
                    model.DefaultMemberName;
            }
            else
            {
                string propertyType =
                    model.Property.Type.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat);

                defaultValue =
                    "default(" +
                    propertyType +
                    ")";
            }

            string options =
                "(global::System.Windows.FrameworkPropertyMetadataOptions)" +
                model.Options.ToString(
                    CultureInfo.InvariantCulture);

            string callback =
                string.IsNullOrEmpty(
                    model.ChangedCallback)
                    ? "null"
                    : model.ChangedCallback;

            return
                "new global::System.Windows.FrameworkPropertyMetadata(" +
                defaultValue +
                ", " +
                options +
                ", " +
                callback +
                ")";
        }

        private static void GeneratePropertyImplementation(
            StringBuilder source,
            string indent,
            PropertyModel model)
        {
            IPropertySymbol property = model.Property;

            string propertyType =
                property.Type.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat);

            // 스키마 속성의 설명을 생성된 실제 CLR 속성에도 전달한다.
            // 인터페이스에만 남기면 Visual Studio 속성 창/IntelliSense에서 확인할 수 없다.
            string documentation = property.GetDocumentationCommentXml();
            if(!string.IsNullOrWhiteSpace(documentation))
            {
                foreach(string line in documentation.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = line.Trim();
                    if(trimmed.StartsWith("<member", StringComparison.Ordinal) || trimmed == "</member>") continue;
                    source.Append(indent).Append("/// ").AppendLine(trimmed);
                }
            }
            foreach(AttributeData attribute in property.GetAttributes())
            {
                if(attribute.AttributeClass == null ||
                   attribute.AttributeClass.ToDisplayString() != "Lincsolution.UI.Controls.LocalizedDescriptionAttribute" ||
                   attribute.ConstructorArguments.Length != 2) continue;
                source.Append(indent).Append("[global::Lincsolution.UI.Controls.LocalizedDescriptionAttribute(");
                source.Append(Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral((string)attribute.ConstructorArguments[0].Value ?? string.Empty, true));
                source.Append(", ");
                source.Append(Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral((string)attribute.ConstructorArguments[1].Value ?? string.Empty, true));
                source.AppendLine(")]");
            }

            source.Append(indent);

            source.Append(
                GetAccessibility(
                    property.DeclaredAccessibility));

            source.Append(propertyType);
            source.Append(" ");
            source.Append(property.Name);

            source.AppendLine();

            source.Append(indent);
            source.AppendLine("{");

            source.Append(indent);
            source.Append("    get => (");
            source.Append(propertyType);
            source.Append(")GetValue(");
            source.Append(property.Name);
            source.AppendLine("Property);");

            source.Append(indent);
            source.Append("    set => SetValue(");
            source.Append(property.Name);
            source.AppendLine("Property, value);");

            source.Append(indent);
            source.AppendLine("}");
        }

        private static string FormatAttributeDefaultValue(
            TypedConstant constant,
            ITypeSymbol propertyType)
        {
            if(constant.IsNull)
                return "null";

            string expression;

            if(constant.Kind ==
                TypedConstantKind.Type)
            {
                ITypeSymbol type =
                    constant.Value as ITypeSymbol;

                if(type == null)
                    return "null";

                expression =
                    "typeof(" +
                    type.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat) +
                    ")";
            }
            else if(constant.Kind ==
                     TypedConstantKind.Enum)
            {
                string enumType =
                    constant.Type != null
                        ? constant.Type.ToDisplayString(
                            SymbolDisplayFormat.FullyQualifiedFormat)
                        : propertyType.ToDisplayString(
                            SymbolDisplayFormat.FullyQualifiedFormat);

                expression =
                    "(" +
                    enumType +
                    ")" +
                    Convert.ToString(
                        constant.Value,
                        CultureInfo.InvariantCulture);
            }
            else
            {
                expression =
                    FormatPrimitive(
                        constant.Value);
            }

            string targetType =
                propertyType.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat);

            /*
             * Attribute의 object 값은 Roslyn상 원래
             * primitive 타입으로 들어올 수 있으므로
             * property 실제 타입으로 명시적 cast한다.
             *
             * 예:
             *
             * byte Value
             * DefaultValue = 10
             *
             * -> (byte)(10)
             */
            return
                "(" +
                targetType +
                ")(" +
                expression +
                ")";
        }

        private static string FormatPrimitive(
            object value)
        {
            if(value == null)
                return "null";

            if(value is string)
            {
                return "\"" +
                    EscapeString(
                        (string)value) +
                    "\"";
            }

            if(value is char)
            {
                return "'" +
                    EscapeChar(
                        (char)value) +
                    "'";
            }

            if(value is bool)
            {
                return (bool)value
                    ? "true"
                    : "false";
            }

            if(value is byte)
            {
                return ((byte)value).ToString(
                    CultureInfo.InvariantCulture);
            }

            if(value is sbyte)
            {
                return ((sbyte)value).ToString(
                    CultureInfo.InvariantCulture);
            }

            if(value is short)
            {
                return ((short)value).ToString(
                    CultureInfo.InvariantCulture);
            }

            if(value is ushort)
            {
                return ((ushort)value).ToString(
                    CultureInfo.InvariantCulture);
            }

            if(value is int)
            {
                return ((int)value).ToString(
                    CultureInfo.InvariantCulture);
            }

            if(value is uint)
            {
                return ((uint)value).ToString(
                           CultureInfo.InvariantCulture)
                       + "U";
            }

            if(value is long)
            {
                return ((long)value).ToString(
                           CultureInfo.InvariantCulture)
                       + "L";
            }

            if(value is ulong)
            {
                return ((ulong)value).ToString(
                           CultureInfo.InvariantCulture)
                       + "UL";
            }

            if(value is float)
            {
                float number = (float)value;

                if(float.IsNaN(number))
                    return "float.NaN";

                if(float.IsPositiveInfinity(number))
                    return "float.PositiveInfinity";

                if(float.IsNegativeInfinity(number))
                    return "float.NegativeInfinity";

                return number.ToString(
                           "R",
                           CultureInfo.InvariantCulture)
                       + "F";
            }

            if(value is double)
            {
                double number = (double)value;

                if(double.IsNaN(number))
                    return "double.NaN";

                if(double.IsPositiveInfinity(number))
                    return "double.PositiveInfinity";

                if(double.IsNegativeInfinity(number))
                    return "double.NegativeInfinity";

                return number.ToString(
                           "R",
                           CultureInfo.InvariantCulture)
                       + "D";
            }

            return Convert.ToString(
                value,
                CultureInfo.InvariantCulture);
        }

        private static string EscapeString(
            string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t")
                .Replace("\0", "\\0");
        }

        private static string EscapeChar(
            char value)
        {
            switch(value)
            {
                case '\\':
                    return "\\\\";

                case '\'':
                    return "\\'";

                case '\r':
                    return "\\r";

                case '\n':
                    return "\\n";

                case '\t':
                    return "\\t";

                case '\0':
                    return "\\0";

                default:
                    return value.ToString();
            }
        }

        private static List<INamedTypeSymbol>
            GetContainingTypes(
                INamedTypeSymbol type)
        {
            List<INamedTypeSymbol> result =
                new List<INamedTypeSymbol>();

            INamedTypeSymbol current = type;

            while(current != null)
            {
                result.Add(current);
                current = current.ContainingType;
            }

            result.Reverse();

            return result;
        }

        private static string GetTypeDeclarationName(
            INamedTypeSymbol type)
        {
            if(type.TypeParameters.Length == 0)
                return type.Name;

            return
                type.Name +
                "<" +
                string.Join(
                    ", ",
                    type.TypeParameters
                        .Select(
                            p => p.Name)) +
                ">";
        }

        private static string GetAccessibility(
            Accessibility accessibility)
        {
            switch(accessibility)
            {
                case Accessibility.Public:
                    return "public ";

                case Accessibility.Internal:
                    return "internal ";

                case Accessibility.Private:
                    return "private ";

                case Accessibility.Protected:
                    return "protected ";

                case Accessibility.ProtectedAndInternal:
                    return "private protected ";

                case Accessibility.ProtectedOrInternal:
                    return "protected internal ";

                default:
                    return string.Empty;
            }
        }

        private static string CreateHintName(
            INamedTypeSymbol owner,
            IPropertySymbol property)
        {
            string name =
                owner.ToDisplayString() +
                "_" +
                property.Name;

            name = name
                .Replace(".", "_")
                .Replace("+", "_")
                .Replace("<", "_")
                .Replace(">", "_")
                .Replace(",", "_")
                .Replace(" ", "_");

            return name + ".DependencyProperty.g.cs";
        }

        private sealed class PropertyModel
        {
            public INamedTypeSymbol Owner
            {
                get; private set;
            }

            public IPropertySymbol Property
            {
                get; private set;
            }

            public bool HasAttributeDefaultValue
            {
                get;
                private set;
            }

            public TypedConstant AttributeDefaultValue
            {
                get;
                private set;
            }

            public long Options
            {
                get; private set;
            }

            public string ChangedCallback
            {
                get;
                private set;
            }

            public string DefaultMemberName
            {
                get;
                private set;
            }

            public PropertyModel(
                INamedTypeSymbol owner,
                IPropertySymbol property,
                bool hasAttributeDefaultValue,
                TypedConstant attributeDefaultValue,
                long options,
                string changedCallback,
                string defaultMemberName)
            {
                Owner = owner;
                Property = property;

                HasAttributeDefaultValue =
                    hasAttributeDefaultValue;

                AttributeDefaultValue =
                    attributeDefaultValue;

                Options = options;

                ChangedCallback =
                    changedCallback;

                DefaultMemberName =
                    defaultMemberName;
            }
        }
    }
}
