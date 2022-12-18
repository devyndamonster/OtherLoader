using OtherLoader.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Controllers
{
    public class ItemSpawnerController : IItemSpawnerController
    {
        public ItemSpawnerState GetInitialState()
        {
            return new ItemSpawnerState
            {
                Page = FistVR.ItemSpawnerV2.PageMode.MainMenu
            };
        }
    }
}
