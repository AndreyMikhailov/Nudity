using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Nudity.Generator
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<MemberAccessExpressionSyntax> MemberAccesses { get; } = new();
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax
                {
                    Name: IdentifierNameSyntax
                    {
                        Identifier:
                        {
                            Text: "AsExposed"
                        }
                    }
                } memberAccess
            })
            {
                MemberAccesses.Add(memberAccess);
            }
        }
    }
}