using System.IO;
using System.Reflection;

namespace Nudity.Utils
{
    internal static class EmbeddedResource
    {
        public static string GetContent(string relativePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var baseName = assembly.GetName().Name;
            
            var resourceName = relativePath
                .TrimStart('.')
                .Replace(Path.DirectorySeparatorChar, '.')
                .Replace(Path.AltDirectorySeparatorChar, '.');

            var fullResourceName = $"{baseName}.{resourceName}";
            using var stream = assembly.GetManifestResourceStream(fullResourceName);

            if (stream == null)
                throw new FileNotFoundException("Embedded resource not found.", fullResourceName);

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}