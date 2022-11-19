using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader
{
    public class EntryNode
    {
        public ItemSpawnerEntry entry;

        public List<EntryNode> childNodes = new List<EntryNode>();

        public EntryNode()
        {
            this.entry = ScriptableObject.CreateInstance<ItemSpawnerEntry>();
        }

        public EntryNode(ItemSpawnerEntry entry)
        {
            this.entry = entry;
        }

    }
}
