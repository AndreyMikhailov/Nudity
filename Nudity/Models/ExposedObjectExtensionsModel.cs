using System.Collections.Generic;
using System.Linq;

namespace Nudity.Models
{
    internal class ExposedObjectExtensionsModel
    {
        public IEnumerable<AsExposedModel> Methods { get; set; } = Enumerable.Empty<AsExposedModel>();
    }
}