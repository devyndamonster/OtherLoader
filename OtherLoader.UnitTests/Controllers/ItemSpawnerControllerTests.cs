using NUnit.Framework;
using OtherLoader.Core.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.UnitTests.Controllers
{
    public class ItemSpawnerControllerTests
    {
        public class GetInitialState
        {
            [Test]
            public void ItWillStartOnMainMenu()
            {
                var itemSpawnerController = new ItemSpawnerController();

                var state = itemSpawnerController.GetInitialState();
                
                Assert.IsTrue(state.Page == FistVR.ItemSpawnerV2.PageMode.MainMenu);
            }
        }
    }
}
