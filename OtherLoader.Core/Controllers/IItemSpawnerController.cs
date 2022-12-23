using OtherLoader.Core.Models;

namespace OtherLoader.Core.Controllers
{
    public interface IItemSpawnerController
    {
        public ItemSpawnerState GetInitialState();

        public ItemSpawnerState PageSelected(ItemSpawnerState state, PageMode page);

        public ItemSpawnerState NextPageClicked(ItemSpawnerState state);

        public ItemSpawnerState PreviousPageClicked(ItemSpawnerState state);
    }
}
