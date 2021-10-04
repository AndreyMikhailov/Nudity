using System.Collections.Generic;
using System.Linq;

namespace Nudity.Models
{
    internal record ExposedObjectAncestorModel
    {
        public string NamespaceName = "";
        public string ClassName = "";
        public IEnumerable<FieldModel> Fields = Enumerable.Empty<FieldModel>();
    }
}