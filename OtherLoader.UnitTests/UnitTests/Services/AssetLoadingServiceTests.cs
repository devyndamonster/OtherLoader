using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using OtherLoader.Core.Adapters;
using OtherLoader.Core.Controllers;
using OtherLoader.Core.Models;
using OtherLoader.Core.Services;
using OtherLoader.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OtherLoader.UnitTests.Services
{
    public class AssetLoadingServiceTests
    {
        [TestFixture]
        public class LoadDirectAssets
        {
            [Test]
            public void ItWillPassLoadedAssetsByFiringEvent()
            {
                var loadedAssets = new object[]
                {
                    "Test Object"
                };

                var modData = new DirectLoadModData
                {
                    FolderPath = "TestPath",
                    Guid = "TestGuid",
                    Dependancies = new string[] { },
                    LoadFirst = new string[] { "TestLoadFirst" },
                    LoadAny = new string[] { },
                    LoadLast = new string[] { },
                };
                
                var returnedAssets = new List<object>();

                var mockLoadOrderService = Substitute.For<ILoadOrderController>();
                mockLoadOrderService
                    .CanBundleBeginLoading(Arg.Any<string>())
                    .Returns(true);
                
                var mockBundleLoadingAdapter = Substitute.For<IBundleLoadingAdapter>();
                mockBundleLoadingAdapter
                    .LoadAssetsFromAssetBundle(Arg.Any<string>())
                    .Returns(MockedLoadAssetsFromBundle(loadedAssets));

                var assetLoadingService = new AssetLoadingService(mockLoadOrderService, mockBundleLoadingAdapter);
                var coroutineUnderTest = assetLoadingService.LoadDirectAssets(modData).First();

                assetLoadingService.OnAssetLoadComplete += objects => returnedAssets.AddRange(objects);
                coroutineUnderTest.ExecuteCoroutine();
                
                returnedAssets.ShouldBeEquivalentTo(loadedAssets);
            }

            [Test]
            public void ItWillReturnCoroutineForEveryBundle()
            {
                var modData = new DirectLoadModData
                {
                    FolderPath = "TestPath",
                    Guid = "TestGuid",
                    Dependancies = new string[] { },
                    LoadFirst = new string[] { "TestLoadFirst", "AnotherLoadFirst" },
                    LoadAny = new string[] { "TestLoadAny" },
                    LoadLast = new string[] { "TestLoadLast" },
                };
                var mockLoadOrderService = Substitute.For<ILoadOrderController>();
                var mockBundleLoadingAdapter = Substitute.For<IBundleLoadingAdapter>();

                var assetLoadingService = new AssetLoadingService(mockLoadOrderService, mockBundleLoadingAdapter);

                var result = assetLoadingService.LoadDirectAssets(modData);
                
                result.Should().HaveCount(4);
            }

            [Test]
            public void ItWontLoadModWhenWaitingForOtherMods()
            {
                var loadedAssets = new object[]
                {
                    "Test Object"
                };

                var modData = new DirectLoadModData
                {
                    FolderPath = "TestPath",
                    Guid = "TestGuid",
                    Dependancies = new string[] { },
                    LoadFirst = new string[] { "TestLoadFirst" },
                    LoadAny = new string[] { },
                    LoadLast = new string[] { },
                };
                
                var mockLoadOrderService = Substitute.For<ILoadOrderController>();
                mockLoadOrderService
                    .CanBundleBeginLoading(Arg.Any<string>())
                    .Returns(false);

                var mockBundleLoadingAdapter = Substitute.For<IBundleLoadingAdapter>();
                mockBundleLoadingAdapter
                    .LoadAssetsFromAssetBundle(Arg.Any<string>())
                    .Returns(MockedLoadAssetsFromBundle(loadedAssets));

                var assetLoadingService = new AssetLoadingService(mockLoadOrderService, mockBundleLoadingAdapter);
                var coroutineUnderTest = assetLoadingService.LoadDirectAssets(modData).First();
                
                coroutineUnderTest.ExecuteCoroutine(99);

                mockLoadOrderService.DidNotReceive().RegisterBundleLoadingStarted(Arg.Any<string>());
            }

            private IEnumerator MockedLoadAssetsFromBundle(object[] assets)
            {
                yield return null;
                yield return assets;
            }
            
        }

    }
}
