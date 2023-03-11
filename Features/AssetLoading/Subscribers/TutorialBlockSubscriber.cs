using FistVR;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OtherLoader.Features.AssetLoading.Subscribers
{
    public class TutorialBlockSubscriber : BaseAssetLoadingSubscriber<TutorialBlock>
    {
        public TutorialBlockSubscriber(IAssetLoadingService assetLoadingService) : base(assetLoadingService) { }

        protected override void LoadSubscribedAssets(IEnumerable<TutorialBlock> assets)
        {
            foreach(var tutorialBlock in assets)
            {
                if (UsesLocalVideo(tutorialBlock))
                {
                    tutorialBlock.MediaRef.MediaPath.Path = ""; //TODO get local path
                }

                IM.TutorialBlockDic[tutorialBlock.ID] = tutorialBlock;
            }
        }

        private bool UsesLocalVideo(TutorialBlock tutorialBlock)
        {
            return string.IsNullOrEmpty(tutorialBlock.MediaRef.MediaPath.Path);
        }
    }
}
