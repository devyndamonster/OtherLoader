using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Tests
{
    public class ItemSpawnerEntrySampleBuilder
    {

        private ItemSpawnerEntry _builtEntry = new ItemSpawnerEntry();

        public ItemSpawnerEntrySampleBuilder()
        {
            Reset();
        }

        public ItemSpawnerEntrySampleBuilder Reset()
        {
            _builtEntry = new ItemSpawnerEntry();
            return this;
        }


        public ItemSpawnerEntry GetEntry()
        {
            return _builtEntry;
        }

        public ItemSpawnerEntrySampleBuilder SetItem(string path, string itemId)
        {
            _builtEntry.EntryPath = path;
            _builtEntry.MainObjectID = itemId;

            return this;
        }

        public ItemSpawnerEntrySampleBuilder SetCategory(string path)
        {
            _builtEntry.EntryPath = path;
            _builtEntry.MainObjectID = "";

            return this;
        }

        public ItemSpawnerEntrySampleBuilder SetIsDisplayed(bool display)
        {
            _builtEntry.IsDisplayedInMainEntry = display;

            return this;
        }

        public ItemSpawnerEntrySampleBuilder SetIsModded(bool isModded)
        {
            _builtEntry.IsModded = isModded;

            return this;
        }
        
        public ItemSpawnerEntrySampleBuilder SetIsReward(bool isReward)
        {
            _builtEntry.IsReward = isReward;

            return this;
        }

        public ItemSpawnerEntrySampleBuilder SetDisplayName(string displayName)
        {
            _builtEntry.DisplayName = displayName;

            return this;
        }

        public ItemSpawnerEntrySampleBuilder SetSpawnWithItems(params string[] itemIds)
        {
            _builtEntry.SpawnWithIDs = new List<string>(itemIds);

            return this;
        }

        public ItemSpawnerEntrySampleBuilder SetSecondaryItems(params string[] itemIds)
        {
            _builtEntry.SecondaryObjectIDs = new List<string>(itemIds);

            return this;
        }

        public ItemSpawnerEntrySampleBuilder SetModTags(params string[] modTags)
        {
            _builtEntry.ModTags = new List<string>(modTags);

            return this;
        }


    }
}
