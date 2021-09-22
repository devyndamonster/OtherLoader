using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader
{
    public class ItemSpawnerCategorySerializable
    {
        public List<CategorySerializable> Categories;

        public ItemSpawnerCategorySerializable() { }

        public ItemSpawnerCategorySerializable(ItemSpawnerCategoryDefinitions cat)
        {
            Categories = cat.Categories.Select(o => new CategorySerializable(o)).ToList();
        }

        public ItemSpawnerCategoryDefinitions GetCategoryDefinitions()
        {
            ItemSpawnerCategoryDefinitions cat = new ItemSpawnerCategoryDefinitions();

            cat.Categories = Categories.Select(o => o.GetCategory()).ToArray();

            return cat;
        }
    }

    public class CategorySerializable
    {
        public ItemSpawnerID.EItemCategory Cat;
        public string DisplayName;
        public List<SubCategorySerializable> Subcats;
        public bool DoesDisplay_Sandbox = true;
        public bool DoesDisplay_Unlocks = true;

        public CategorySerializable() { }

        public CategorySerializable(ItemSpawnerCategoryDefinitions.Category cat)
        {
            Cat = cat.Cat;
            DisplayName = cat.DisplayName;
            Subcats = cat.Subcats.Select(o => new SubCategorySerializable(o)).ToList();
            DoesDisplay_Sandbox = cat.DoesDisplay_Sandbox;
            DoesDisplay_Unlocks = cat.DoesDisplay_Unlocks;
        }

        public ItemSpawnerCategoryDefinitions.Category GetCategory()
        {
            ItemSpawnerCategoryDefinitions.Category cat = new ItemSpawnerCategoryDefinitions.Category();

            cat.Cat = Cat;
            cat.DisplayName = DisplayName;
            cat.Subcats = Subcats.Select(o => o.GetSubCategory()).ToArray();
            cat.DoesDisplay_Sandbox = DoesDisplay_Sandbox;
            cat.DoesDisplay_Unlocks = DoesDisplay_Unlocks;

            return cat;
        }

    }

    public class SubCategorySerializable
    {
        public ItemSpawnerID.ESubCategory Subcat;
        public string DisplayName;
        public bool DoesDisplay_Sandbox = true;
        public bool DoesDisplay_Unlocks = true;

        public SubCategorySerializable() { }

        public SubCategorySerializable(ItemSpawnerCategoryDefinitions.SubCategory cat) 
        {
            Subcat = cat.Subcat;
            DisplayName = cat.DisplayName;
            DoesDisplay_Sandbox = cat.DoesDisplay_Sandbox;
            DoesDisplay_Unlocks = cat.DoesDisplay_Unlocks;
        }

        public ItemSpawnerCategoryDefinitions.SubCategory GetSubCategory()
        {
            ItemSpawnerCategoryDefinitions.SubCategory cat = new ItemSpawnerCategoryDefinitions.SubCategory();

            cat.Subcat = Subcat;
            cat.DisplayName = DisplayName;
            cat.DoesDisplay_Sandbox = DoesDisplay_Sandbox;
            cat.DoesDisplay_Unlocks = DoesDisplay_Unlocks;

            return cat;
        }
    }

}
