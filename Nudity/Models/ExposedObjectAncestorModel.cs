using System.Collections.Generic;
using System.Linq;

namespace Nudity.Models
{
    internal record ExposedObjectAncestorModel
    {
        public string NamespaceName { get; set; } = "";
        public string ClassName { get; set; } = "";
        public IEnumerable<FieldModel> Fields { get; set; } = Enumerable.Empty<FieldModel>();
    }
}