using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Models
{
    public interface IAssetLoadingSubscriber
    {
        public void LoadSubscribedAssets(object[] assets);
    }
}
