using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Models
{
    public abstract class BaseAssetLoadingSubscriber<T>
    {
        public void LoadSubscribedAssets(object[] assets)
        {
            LoadSubscribedAssets(assets.OfType<T>());
        }

        protected abstract void LoadSubscribedAssets(IEnumerable<T> assets);
    }
}
