using FistVR;
using OtherLoader.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OtherLoader.Loaders
{
    public class CategoryDefinitionLoader : BaseAssetLoader
    {
        private readonly IMetaDataService _metaDataService = new MetaDataService(new PathService());

        public override IEnumerator LoadAssetsFromBundle(AssetBundle assetBundle, string bundleId)
        {
            return LoadAssetsFromBundle<ItemSpawnerCategoryDefinitions>(assetBundle, bundleId);
        }

        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            ItemSpawnerCategoryDefinitions catDef = asset as ItemSpawnerCategoryDefinitions;

            foreach (ItemSpawnerCategoryDefinitions.Category newCategory in catDef.Categories)
            {
                LoadCategory(newCategory);
            }
        }


        private void LoadCategory(ItemSpawnerCategoryDefinitions.Category category)
        {
            OtherLogger.Log("Loading New ItemSpawner Category! Name (" + category.DisplayName + "), Value (" + category.Cat + ")", OtherLogger.LogType.Loading);

            if (DoesCategoryAlreadyExist(category.Cat))
            {
                OtherLogger.Log("Category already exists! Adding subcategories", OtherLogger.LogType.Loading);
                MergeCategoryWithExisting(category);
            }

            else
            {
                OtherLogger.Log("This is a new primary category", OtherLogger.LogType.Loading);
                AddNewCategory(category);
            }
        }


        private void AddNewCategory(ItemSpawnerCategoryDefinitions.Category category)
        {
            AddCategoryToGlobalDictionaries(category);
            
            foreach (ItemSpawnerCategoryDefinitions.SubCategory newSubCat in category.Subcats)
            {
                AddSubcategoryToGlobalDictionaries(newSubCat, category.Cat);
            }
        }



        private void MergeCategoryWithExisting(ItemSpawnerCategoryDefinitions.Category category)
        {
            ItemSpawnerCategoryDefinitions.Category originalCat = IM.CDefs.Categories.FirstOrDefault(o => o.Cat == category.Cat);

            foreach (ItemSpawnerCategoryDefinitions.SubCategory newSubCat in category.Subcats)
            {
                //Only add this new subcategory if it is unique
                if (!IM.CDefSubInfo.ContainsKey(newSubCat.Subcat))
                {
                    OtherLogger.Log("Adding subcategory: " + newSubCat.DisplayName, OtherLogger.LogType.Loading);

                    AddSubcatToCategory(originalCat, newSubCat);
                    AddSubcategoryToGlobalDictionaries(newSubCat, originalCat.Cat);
                }

                else
                {
                    OtherLogger.LogWarning("SubCategory type is already being used, and SubCategory will not be added! Make sure your subcategory is using a unique type! SubCategory Type: " + newSubCat.Subcat);
                }
            }
        }

        private void AddSubcatToCategory(ItemSpawnerCategoryDefinitions.Category category, ItemSpawnerCategoryDefinitions.SubCategory subcategory)
        {
            category.Subcats = category.Subcats.Concat(new[] { subcategory }).ToArray();
        }

        private void AddSubcategoryToGlobalDictionaries(ItemSpawnerCategoryDefinitions.SubCategory subcategory, ItemSpawnerID.EItemCategory parentCategory)
        {
            IM.CDefSubs[parentCategory].Add(subcategory);
            IM.CDefSubInfo.AddIfUnique(subcategory.Subcat, subcategory);
            IM.SCD.AddIfUnique(subcategory.Subcat, new List<ItemSpawnerID>());

            var tag = _metaDataService.GetTagFromSubcategory(subcategory.Subcat);
            
            OtherLoader.TagGroupsByTag[tag] = new ItemSpawnerCategoryDefinitionsV2.SpawnerPage.SpawnerTagGroup
            {
                DisplayName = subcategory.DisplayName,
                Tag = tag,
                TagT = TagType.SubCategory,
                Icon = subcategory.Sprite
            };
        }

        private void AddCategoryToGlobalDictionaries(ItemSpawnerCategoryDefinitions.Category category)
        {
            IM.CDefs.Categories = IM.CDefs.Categories.Concat(new[] { category }).ToArray();
            IM.CD.CreateValueIfNewKey(category.Cat);
            IM.CDefSubs.CreateValueIfNewKey(category.Cat);
            IM.CDefInfo.AddIfUnique(category.Cat, category);

            var tag = _metaDataService.GetTagFromCategory(category.Cat);

            OtherLoader.TagGroupsByTag[tag] = new ItemSpawnerCategoryDefinitionsV2.SpawnerPage.SpawnerTagGroup
            {
                DisplayName = category.DisplayName,
                Tag = tag,
                TagT = TagType.Category,
                Icon = category.Sprite
            };
        }


        private bool DoesCategoryAlreadyExist(ItemSpawnerID.EItemCategory category)
        {
            return IM.CDefs.Categories.Any(o => o.Cat == category);
        }



    }
}
