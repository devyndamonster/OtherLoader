using FistVR;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;
using System;
using System.Collections.Generic;

namespace OtherLoader.Features.AssetLoading.Subscribers
{
    public class ItemSpawnerIdSubscriber : BaseAssetLoadingSubscriber<ItemSpawnerID>
    {
        public ItemSpawnerIdSubscriber(IAssetLoadingService assetLoadingService) : base(assetLoadingService)
        {
        }

        protected override void LoadSubscribedAssets(IEnumerable<ItemSpawnerID> assets)
        {
            /* What Do We Do With SpawnerIds?
             * - Parse them into a unified item spawner entry
             * - Pass them into an item meta data store
             * - Assets like images get connected by Id
             * - Item spawner controller hooks into item meta data store?
             */
        }
    }
}
