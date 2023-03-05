using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Features.AssetLoading.Models
{
    public class ModLoadData
    {
        public string ModId { get; set; }

        public List<string> BundlePaths { get; set; }
    }
}
