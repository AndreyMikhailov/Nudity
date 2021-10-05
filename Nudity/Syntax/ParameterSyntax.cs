namespace Nudity.Syntax
{
    internal class ParameterSyntax
    {
        public ParameterSyntax(TypeSyntax type, OutputKind outputKind)
        {
            Type = type;
            OutputKind = outputKind;
        }

        public TypeSyntax Type { get; }
        public OutputKind OutputKind { get; }
    }
}