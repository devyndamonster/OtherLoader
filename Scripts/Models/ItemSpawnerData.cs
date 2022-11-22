using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader
{
    public class ItemSpawnerData : MonoBehaviour
    {
        public string CurrentPath;

        public List<ItemSpawnerEntry> VisibleEntries = new List<ItemSpawnerEntry>();

        public List<ItemSpawnerEntry> VisibleSecondaryEntries = new List<ItemSpawnerEntry>();

        public int CurrentDepth = 0;

        public Dictionary<ItemSpawnerV2.PageMode, Dictionary<int, int>> SavedPagePositions = new Dictionary<ItemSpawnerV2.PageMode, Dictionary<int, int>>();
    }
}
