using FistVR;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Scripts.Loading.Subscribers
{
    public class TutorialBlockSubscriber : BaseAssetLoadingSubscriber<TutorialBlock>
    {
        public TutorialBlockSubscriber(IAssetLoadingService assetLoadingService) : base(assetLoadingService) { }

        protected override void LoadSubscribedAssets(IEnumerable<TutorialBlock> assets)
        {
            throw new NotImplementedException();
        }
    }
}
