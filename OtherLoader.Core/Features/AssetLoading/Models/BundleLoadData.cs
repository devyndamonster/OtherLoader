using OtherLoader.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Features.AssetLoading.Models
{
    public class BundleLoadData
    {
        public BundleLoadState LoadState { get; set; }

        public LoadOrderType LoadOrder { get; set; }

        public string BundlePath { get; set; }

        public string ModId { get; set; }

    }
}
