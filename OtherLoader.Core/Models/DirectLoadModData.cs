using OtherLoader.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Models
{
    public class DirectLoadModData
    {
        public string Path { get; set; }

        public string Guid { get; set; }

        public string[] Dependancies { get; set; }

        public string[] LoadFirst { get; set; }

        public string[] LoadAny { get; set; }
        
        public string[] LoadLast { get; set; }

        public Dictionary<LoadOrderType, string[]> BundlesByLoadOrder => new Dictionary<LoadOrderType, string[]>
        {
            [LoadOrderType.LoadFirst] = LoadFirst,
            [LoadOrderType.LoadUnordered] = LoadAny,
            [LoadOrderType.LoadLast] = LoadLast
        };
    }
}
