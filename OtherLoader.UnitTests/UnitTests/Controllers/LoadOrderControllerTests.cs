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
            [Test]
            public void LoadFirstWontLoad_WhileLoadFirstDependancyWaiting()
            {
                var loadOrderController = new LoadOrderController();
                loadOrderController.RegisterBundleForLoading("BundleA", "ModA", LoadOrderType.LoadFirst);
                loadOrderController.RegisterBundleForLoading("BundleB", "ModA", LoadOrderType.LoadFirst);

                var result = loadOrderController.CanBundleBeginLoading("BundleB");

                result.Should().BeFalse();
            }

            [Test]
            public void LoadFirstWontLoad_WhileLoadFirstDependancyLoading()
            {
                var loadOrderController = new LoadOrderController();
                loadOrderController.RegisterBundleForLoading("BundleA", "ModA", LoadOrderType.LoadFirst);
                loadOrderController.RegisterBundleForLoading("BundleB", "ModA", LoadOrderType.LoadFirst);
                loadOrderController.RegisterBundleLoadingStarted("BundleA");

                var result = loadOrderController.CanBundleBeginLoading("BundleB");

                result.Should().BeFalse();
            }

            [Test]
            public void LoadFirstWillLoad_WhenLoadFirstDependancyLoaded()
            {
                var loadOrderController = new LoadOrderController();
                loadOrderController.RegisterBundleForLoading("BundleA", "ModA", LoadOrderType.LoadFirst);
                loadOrderController.RegisterBundleForLoading("BundleB", "ModA", LoadOrderType.LoadFirst);
                loadOrderController.RegisterBundleLoadingStarted("BundleA");
                loadOrderController.RegisterBundleLoadingComplete("BundleA");

                var result = loadOrderController.CanBundleBeginLoading("BundleB");

                result.Should().BeTrue();
            }

            [Test]
            public void LoadFirstWillLoad_WhenOtherLoadFirstDependsOnIt()
            {
                var loadOrderController = new LoadOrderController();
                loadOrderController.RegisterBundleForLoading("BundleA", "ModA", LoadOrderType.LoadFirst);
                loadOrderController.RegisterBundleForLoading("BundleB", "ModA", LoadOrderType.LoadFirst);

                var result = loadOrderController.CanBundleBeginLoading("BundleA");

                result.Should().BeTrue();
            }

            [Test]
            public void LoadFirstWillLoad_WhenOtherLoadUnorderedDependsOnIt()
            {
                var loadOrderController = new LoadOrderController();
                loadOrderController.RegisterBundleForLoading("BundleA", "ModA", LoadOrderType.LoadFirst);
                loadOrderController.RegisterBundleForLoading("BundleB", "ModA", LoadOrderType.LoadUnordered);

                var result = loadOrderController.CanBundleBeginLoading("BundleA");

                result.Should().BeTrue();
            }

            [Test]
            public void LoadFirstWillLoad_WhenOtherLoadLastDependsOnIt()
            {
                var loadOrderController = new LoadOrderController();
                loadOrderController.RegisterBundleForLoading("BundleA", "ModA", LoadOrderType.LoadFirst);
                loadOrderController.RegisterBundleForLoading("BundleB", "ModA", LoadOrderType.LoadLast);

                var result = loadOrderController.CanBundleBeginLoading("BundleA");

                result.Should().BeTrue();
            }

            //TODO more tests
        }
    }
}
