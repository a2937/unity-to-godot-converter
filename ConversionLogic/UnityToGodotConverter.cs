using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConversionLogic
{
    /// <summary>
    /// This class provides functionality to convert Unity C# code to Godot C# code.
    /// </summary>
    public class UnityToGodotConverter : CSharpSyntaxRewriter
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

        private static readonly Dictionary<string, string> VariableMapping = new Dictionary<string, string>()
        {
             { "transform", "Transform" },
             {"position", "Origin" }
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
            // Check if the class is extending a Unity base class.
            if (BaseClassMapping.TryGetValue(node!.BaseList?.Types.FirstOrDefault()?.Type.ToString(), out string godotBaseClass))
            {
                // Replace the base class with the corresponding Godot base class.
                node = node.WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(godotBaseClass)))));
            }

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
            // Convert Unity's Start() method to Godot's _Ready() method.
            if (node.Identifier.Text == "Start")
            {
                node = node
                 .WithModifiers(SyntaxFactory.TokenList(
                     SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                     SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
                 .WithIdentifier(SyntaxFactory.Identifier("_Ready"));
            }

            // Convert Unity's Update method to Godot's _Process method.
            else if (node.Identifier.Text == "Update")
            {
                node = node
               .WithModifiers(SyntaxFactory.TokenList(
                   SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                   SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
               .WithIdentifier(SyntaxFactory.Identifier("_Process"));
            }

            // Convert Unity's Update method to Godot's _Process method.
            else if (node.Identifier.Text == "Awake")
            {
                node = node
               .WithModifiers(SyntaxFactory.TokenList(
                   SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                   SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
               .WithIdentifier(SyntaxFactory.Identifier("_EnterTree"));
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
                            if (VariableMapping.ContainsKey(expressionPart))
                            {
                                newExpression += (VariableMapping[expressionPart] + ".");
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
            // Check if it's a GetComponent call
            if (node.Expression.ToString() == "GetComponent")
            {
                // Get the type argument from GetComponent<T>()
                TypeSyntax typeArgument = args.Arguments.FirstOrDefault()?.Expression as TypeSyntax;

                if (typeArgument != null)
                {
                    // Replace with GetNode<T>("PathToNode")
                    string typeName = typeArgument.ToString();
                    string newNodeExpression = $"GetNode<{typeName}>(\"PathToNode\")";

                    // Create a new expression
                    return SyntaxFactory.ParseExpression(newNodeExpression);
                }
            }
            return base.VisitInvocationExpression(node);
        }
    }
}