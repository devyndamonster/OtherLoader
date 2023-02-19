using FluentAssertions;
using NUnit.Framework;
using OtherLoader.Core.Services;
using System.Linq;

namespace OtherLoader.IntegrationTests.Controllers
{
    public class AssetLoadingControllerTests
    {
        [TestFixture]
        public class LoadDirectAssets
        {
            [Test]
            public void WillReturnCoroutineForAllBundles()
            {
                var assetLoadingService = new AssetLoadingService();

                var result = assetLoadingService.LoadDirectAssets("folderPath", "guid", new string[] { "dependancies" }, new string[] { "loadFirst" }, new string[] { "loadAny" }, new string[] { "loadLast" });

                result.Should().HaveCount(3);
            }
        }
    }
}
