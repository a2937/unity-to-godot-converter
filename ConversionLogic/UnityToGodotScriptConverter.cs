using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.AccessControl;

namespace ConversionLogic
{
    /// <summary>
    /// This class provides functionality to convert Unity C# code to Godot C# code.
    /// </summary>
    public class UnityToGodotScriptConverter : CSharpSyntaxRewriter
    {
        /// <summary>
        /// A dictionary that maps Unity namespaces to Godot namespaces.
        /// </summary>
        private static readonly Dictionary<string, string> ImportMapping = new Dictionary<string, string>
        {
            { "UnityEngine", "Godot" },
        };

        /// <summary>
        /// A dictionary that maps Unity base classes to Godot base classes.
        /// Add more mappings as needed.
        /// </summary>
        private static readonly Dictionary<string, string> BaseClassMapping = new Dictionary<string, string>
        {
            { "MonoBehaviour", "Node" },
            // Add more mappings as needed.
        };

        /// <summary>
        /// A dictionary that maps Unity variable classes
        /// to their closest equivalents. 
        /// </summary>
        private static readonly Dictionary<string, string> VariableMapping = new Dictionary<string, string>()
        {
             { "transform", "Transform" },
             {"position", "Origin" },
             { "Sprite", "Texture2D" },
             { "SpriteRenderer","Sprite2D" },
             { "sprite","texture" }
        };


        /// <summary>
        /// A dictionary that maps Unity methods
        /// to their closest equivalents. 
        /// </summary>
        private static readonly Dictionary<string, string> MethodMapping = new Dictionary<string, string>()
        {
            {"Awake","_EnterTree" },
            { "Start", "_Ready" },
            {"Update", "_Process" },
            {"FixedUpdate", "_PhysicsProcess" },
            {"OnTriggerEnter2D","_OnAreaEntered" }
        };

        /// <summary>
        /// A dictionary that maps Unity array types
        /// to their closest equivalents. 
        /// </summary>
        private static readonly Dictionary<string, string> ArrayTypeMapping = new Dictionary<string, string>()
        {
            {"Sprite","Texture2D" },
            { "SpriteRenderer","Sprite2D" },
            { "Animator","AnimationPlayer" },
             { "AudioSource"," AudioStreamPlayer" },
        };

        /// <summary>
        /// Converts Unity C# code to Godot C# code.
        /// </summary>
        /// <param name="unityCode">The Unity C# code to convert.</param>
        /// <returns>The converted Godot C# code.</returns>
        public string ConvertUnityCodeToGodot(string unityCode)
        {
            // Parse Unity C# code into a syntax tree.
            SyntaxTree unitySyntaxTree = CSharpSyntaxTree.ParseText(unityCode);

            // Use the syntax tree to rewrite the code.
            var godotSyntaxTree = (CompilationUnitSyntax)Visit(unitySyntaxTree.GetRoot());

            // Generate the Godot C# code.
            return godotSyntaxTree.NormalizeWhitespace().ToFullString();
        }

        /// <summary>
        /// Overrides the method to handle class declarations and base class replacement.
        /// </summary>
        /// <param name="node">The class declaration syntax node.</param>
        /// <returns>The modified syntax node.</returns>
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            // TODO: Add a disclaimer about some things needing updating 

            // Check if the class is extending a Unity base class.
            if (BaseClassMapping.TryGetValue(node!.BaseList?.Types.FirstOrDefault()?.Type.ToString(), out string godotBaseClass))
            {
                // Replace the base class with the corresponding Godot base class.
                node = node.WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(godotBaseClass)))));
            }

            // Add the 'partial' keyword to class modifiers.
            node = node.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)));

            // Continue visiting other nodes in the class.
            return base.VisitClassDeclaration(node);
        }


        /// <summary>
        /// Overrides the method to handle using directives and namespace replacement.
        /// </summary>
        /// <param name="node">The using directive syntax node.</param>
        /// <returns>The modified syntax node.</returns>
        public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
        {
            string originalNamespace = node.Name.ToString();
            if (ImportMapping.TryGetValue(originalNamespace, out string godotNamespace))
            {
                // Replace Unity's namespace with Godot's namespace.
                return node.WithName(SyntaxFactory.ParseName(godotNamespace));
            }

            return base.VisitUsingDirective(node);
        }


        /// <summary>
        /// Overrides the method to handle method declarations and method body replacement.
        /// </summary>
        /// <param name="node">The method declaration syntax node.</param>
        /// <returns>The modified syntax node.</returns>
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            string originalMethodName = node.Identifier.Text.ToString();
            if (MethodMapping.TryGetValue(originalMethodName, out string godotMethod))
            {
                node = node
                .WithModifiers(SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
                .WithIdentifier(SyntaxFactory.Identifier(godotMethod));

                // Add a parameter of type double if it's the Update method
                if (originalMethodName == "Update" || originalMethodName == "FixedUpdate")
                {
                    // Create a parameter with type double
                    var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("deltaTime"))
                                                    .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword)));

                    // Add the parameter to the method's parameter list
                    node = node.AddParameterListParameters(parameter);
                }
            }

            // Visit the method body and replace "Debug.Log" with "GD.Print"
            var newBody = (BlockSyntax)Visit(node.Body);



            // Update the method body
            return node.WithBody(newBody);
        }

        // Overrides the method to handle member access expressions and replaces Unity-specific access with Godot ones
        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            // Check if it's accessing "transform.position.x" or "transform.position.y"
            if (node.Expression is MemberAccessExpressionSyntax parentExpression &&
                parentExpression.Expression is MemberAccessExpressionSyntax grandparentExpression &&
                grandparentExpression.Expression.ToString() == "transform" &&
                grandparentExpression.Name.Identifier.Text == "position")
            {
                if (node.Name.Identifier.Text == "x")
                {
                    // Replace with "Transform.Origin.X"
                    return SyntaxFactory.ParseExpression("Transform.Origin.X");
                }
                else if (node.Name.Identifier.Text == "y")
                {
                    // Replace with "Transform.Origin.Y"
                    return SyntaxFactory.ParseExpression("Transform.Origin.Y");
                }
            }

            return base.VisitMemberAccessExpression(node);
        }

        /// <summary>
        /// Overrides the method to handle array expressions and replaces Unity-specific access with Godot ones
        /// </summary>
        /// <param name="node">An array expression</param>
        /// <returns></returns>
        public override SyntaxNode VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
        {
            var originalTypeName = node.GetType().Name;
            // Process array type here
            if (ArrayTypeMapping.TryGetValue(originalTypeName, out string godotType))
            {
                node = node.WithType(SyntaxFactory.ArrayType(SyntaxFactory.ParseTypeName(godotType)));
            }

            // Call base implementation to continue visiting other nodes
            return base.VisitArrayCreationExpression(node);
        }


        /// <summary>
        /// Overrides the method to handle array expressions and replaces Unity-specific access with Godot ones
        /// </summary>
        /// <param name="node">An array expression</param>
        /// <returns></returns>
        public override SyntaxNode VisitArrayType(ArrayTypeSyntax node)
        {
            var originalTypeName = node.ElementType.ToString();
            // Process array type here
            if (ArrayTypeMapping.TryGetValue(originalTypeName, out string godotType))
            {
                node = node.WithElementType(SyntaxFactory.ParseTypeName(godotType));
            }

            // Call base implementation to continue visiting other nodes
            return base.VisitArrayType(node);
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var originalType = node.Declaration.Type.ToString();

            if (ArrayTypeMapping.TryGetValue(originalType, out string godotType))
            {
                var newType = SyntaxFactory.ParseTypeName(godotType);
                // Replace the original type with the new type
                var newDeclaration = node.Declaration.WithType(newType);
                // Create a new field declaration with the updated type
                var newFieldDeclaration = node.WithDeclaration(newDeclaration);

                // Return the modified field declaration
                return newFieldDeclaration;

            }
            // If the field declaration doesn't match, return the original node
            return base.VisitFieldDeclaration(node);
        }

        public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            // Check if the assignment is to enable/disable a node
            if (node.Left is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.Text == "enabled")
            {
                // Check if the left side of the assignment is a member access expression
                if (memberAccess.Expression is IdentifierNameSyntax identifier)
                {
                    // Get the name of the variable being assigned
                    string variableName = identifier.Identifier.Text;


                    ExpressionSyntax invertedValue = SyntaxFactory.ParseExpression($"!{node.Right}");
                    if (node.Right.GetText().ToString() == "true")
                    {
                        invertedValue = SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
                    }
                    else if (node.Right.GetText().ToString() == "false")
                    {
                        invertedValue = SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
                    }

                    // Create the new method invocation expression
                    var newMethodInvocation = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(variableName),
                            SyntaxFactory.IdentifierName("SetDisabled")),
                        SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(invertedValue))));

                    // Return the new method invocation expression
                    return newMethodInvocation;
                }
            }

            // Visit other types of assignment expressions
            return base.VisitAssignmentExpression(node);
        }

        /// <summary>
        /// Overrides the method to handle invocation expressions and replaces
        /// Unity specific calls with Godot ones
        /// </summary>
        /// <param name="node">The invocation expression syntax node.</param>
        /// <returns>The modified syntax node.</returns>
        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var args = node.ArgumentList;
            if (node.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                // Create a list to store the updated arguments
                var updatedArguments = new List<ArgumentSyntax>();

                // Iterate through the arguments
                foreach (var argument in node.ArgumentList.Arguments)
                {
                    // Check if the argument is a MemberAccessExpression representing "transform.position.x"
                    if (argument.Expression is MemberAccessExpressionSyntax argumentMemberAccess)
                    {

                        var expressionParts = argumentMemberAccess.ToString().Split(".");
                        String newExpression = "";
                        foreach (String expressionPart in expressionParts)
                        {
                            if (VariableMapping.TryGetValue(expressionPart, out string? value))
                            {
                                newExpression += value + ".";
                            }
                            else
                            {
                                newExpression += expressionPart.ToUpperInvariant();
                            }
                        }
                        updatedArguments.Add(SyntaxFactory.Argument(SyntaxFactory.ParseExpression($"{newExpression}")));
                    }
                    else
                    {
                        // If not, keep the original argument
                        updatedArguments.Add(argument);
                    }
                    // Create a new argument list with the updated arguments

                    // Create a new expression with the updated argument list
                    args = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(updatedArguments));
                }
                // Check if it's a MemberAccessExpression with "Debug.Log"
                if (
                    memberAccess.Expression.ToString() == "Debug" &&
                    memberAccess.Name.Identifier.Text == "Log")
                {
                    // Replace "Debug.Log" with "GD.Print"
                    return SyntaxFactory.ParseExpression($"GD.Print{args}");
                }
            }

            else if (node.Expression is GenericNameSyntax genericName)
            {
                // Check if it's a GetComponent call
                if (genericName.GetText().ToString().Trim().StartsWith("GetComponent"))
                {
                    // Get the type argument from GetComponent<T>()
                    // Access the list of type arguments (e.g., "T")
                    SeparatedSyntaxList<TypeSyntax> typeArguments = genericName.TypeArgumentList.Arguments;

                    List<string> types = new List<string>();
                    foreach (TypeSyntax typeArgument in typeArguments)
                    {
                        string typeName = typeArgument.ToString();
                        if (ArrayTypeMapping.TryGetValue(typeName, out string godotType))
                        {
                            typeName = godotType;
                        }
                        types.Add(typeName);
                    }

                    // Remove any empty members from the list
                    types.RemoveAll(string.IsNullOrEmpty);

                    string newNodeExpression = $"GetNode<{String.Join(",", types)}>(\".\")";
                    return SyntaxFactory.ParseExpression(newNodeExpression);
                }

            }
            return base.VisitInvocationExpression(node);
        }
    }
}