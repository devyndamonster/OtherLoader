using FluentAssertions;
using NUnit.Framework;
using OtherLoader.Core.Controllers;
using OtherLoader.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.UnitTests.Controllers
{
    public class LoadOrderControllerTests
    {
        [TestFixture]
        public class CanBundleBeginLoading
        {
            [TestCase(LoadOrderType.LoadFirst, LoadOrderType.LoadFirst)]
            [TestCase(LoadOrderType.LoadUnordered, LoadOrderType.LoadFirst)]
            [TestCase(LoadOrderType.LoadLast, LoadOrderType.LoadFirst)]
            [TestCase(LoadOrderType.LoadLast, LoadOrderType.LoadUnordered)]
            [TestCase(LoadOrderType.LoadLast, LoadOrderType.LoadLast)]
            public void BundleWontLoad_WhileDependancyWaiting(LoadOrderType bundleLoadOrder, LoadOrderType dependancyLoadOrder)
            {
                var loadOrderController = new LoadOrderController();
                loadOrderController.RegisterBundleForLoading("BundleA", "ModA", dependancyLoadOrder);
                loadOrderController.RegisterBundleForLoading("BundleB", "ModA", bundleLoadOrder);

                var result = loadOrderController.CanBundleBeginLoading("BundleB");

                result.Should().BeFalse();
            }

            [TestCase(LoadOrderType.LoadFirst, LoadOrderType.LoadFirst)]
            [TestCase(LoadOrderType.LoadUnordered, LoadOrderType.LoadFirst)]
            [TestCase(LoadOrderType.LoadLast, LoadOrderType.LoadFirst)]
            [TestCase(LoadOrderType.LoadLast, LoadOrderType.LoadUnordered)]
            [TestCase(LoadOrderType.LoadLast, LoadOrderType.LoadLast)]
            public void BundleWontLoad_WhileDependancyLoading(LoadOrderType bundleLoadOrder, LoadOrderType dependancyLoadOrder)
            {
                var loadOrderController = new LoadOrderController();
                loadOrderController.RegisterBundleForLoading("BundleA", "ModA", dependancyLoadOrder);
                loadOrderController.RegisterBundleForLoading("BundleB", "ModA", bundleLoadOrder);
                loadOrderController.RegisterBundleLoadingStarted("BundleA");

                var result = loadOrderController.CanBundleBeginLoading("BundleB");

                result.Should().BeFalse();
            }

            [TestCase(LoadOrderType.LoadFirst, LoadOrderType.LoadFirst)]
            [TestCase(LoadOrderType.LoadUnordered, LoadOrderType.LoadFirst)]
            [TestCase(LoadOrderType.LoadLast, LoadOrderType.LoadFirst)]
            [TestCase(LoadOrderType.LoadLast, LoadOrderType.LoadUnordered)]
            [TestCase(LoadOrderType.LoadLast, LoadOrderType.LoadLast)]
            public void BundleWillLoad_WhenDependancyLoaded(LoadOrderType bundleLoadOrder, LoadOrderType dependancyLoadOrder)
            {
                var loadOrderController = new LoadOrderController();
                loadOrderController.RegisterBundleForLoading("BundleA", "ModA", dependancyLoadOrder);
                loadOrderController.RegisterBundleForLoading("BundleB", "ModA", bundleLoadOrder);
                loadOrderController.RegisterBundleLoadingStarted("BundleA");
                loadOrderController.RegisterBundleLoadingComplete("BundleA");

                var result = loadOrderController.CanBundleBeginLoading("BundleB");

                result.Should().BeTrue();
            }

            [TestCase(LoadOrderType.LoadFirst, LoadOrderType.LoadFirst)]
            [TestCase(LoadOrderType.LoadFirst, LoadOrderType.LoadUnordered)]
            [TestCase(LoadOrderType.LoadFirst, LoadOrderType.LoadLast)]
            [TestCase(LoadOrderType.LoadUnordered, LoadOrderType.LoadLast)]
            [TestCase(LoadOrderType.LoadLast, LoadOrderType.LoadLast)]
            public void BundleWillLoad_WhenOtherBundleDependsOnIt(LoadOrderType bundleLoadOrder, LoadOrderType dependantLoadOrder)
            {
                var loadOrderController = new LoadOrderController();
                loadOrderController.RegisterBundleForLoading("BundleA", "ModA", bundleLoadOrder);
                loadOrderController.RegisterBundleForLoading("BundleB", "ModA", dependantLoadOrder);

                var result = loadOrderController.CanBundleBeginLoading("BundleA");

                result.Should().BeTrue();
            }

            [TestCase("BundleA")]
            [TestCase("BundleB")]
            public void LoadUnorderedBundles_WillLoadInAnyOrder(string bundle)
            {
                var loadOrderController = new LoadOrderController();
                loadOrderController.RegisterBundleForLoading("BundleA", "ModA", LoadOrderType.LoadUnordered);
                loadOrderController.RegisterBundleForLoading("BundleB", "ModA", LoadOrderType.LoadUnordered);

                var result = loadOrderController.CanBundleBeginLoading(bundle);

                result.Should().BeTrue();
            }

            [Test]
            public void BundleLoadOrderOnlyAppliesToSameModId()
            {
                var loadOrderController = new LoadOrderController();
                loadOrderController.RegisterBundleForLoading("BundleA", "ModA", LoadOrderType.LoadFirst);
                loadOrderController.RegisterBundleForLoading("BundleB", "ModB", LoadOrderType.LoadUnordered);

                var result = loadOrderController.CanBundleBeginLoading("BundleB");

                result.Should().BeTrue();
            }
        }
    }
}
