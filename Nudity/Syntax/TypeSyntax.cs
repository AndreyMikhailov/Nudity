using System.Collections.Generic;

namespace Nudity.Syntax
{
    internal class TypeSyntax
    {
        public TypeSyntax(string name, string ns, IEnumerable<TypeSyntax> typeArguments)
        {
            Name = name;
            Namespace = ns;
            TypeArguments = typeArguments;
        }

        public string Name { get; }
        public string Namespace { get; }
        public IEnumerable<TypeSyntax> TypeArguments { get; }
    }
}