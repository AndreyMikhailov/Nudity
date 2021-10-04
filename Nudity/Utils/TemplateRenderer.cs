using System.Collections.Generic;
using Scriban;

namespace Nudity.Utils
{
    internal class TemplateRenderer
    {
        private readonly Dictionary<string, Template> _templatesCache = new();
        
        public string Render(string templatePath, object model)
        {
            var template = GetTemplate(templatePath);
            return template.Render(model, member => member.Name);
        }

        private Template GetTemplate(string templatePath)
        {
            if (_templatesCache.TryGetValue(templatePath, out Template templateCache))
                return templateCache;
            
            var content = EmbeddedResource.GetContent(templatePath);
            var template = Template.Parse(content);
            _templatesCache[templatePath] = template;
            
            return template;
        }
    }
}