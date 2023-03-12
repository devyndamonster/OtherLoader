using OtherLoader.Core.Features.MetaData.Models.Vanilla;
using OtherLoader.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Features.MetaData.Services
{
    public class SpawnerEntryDataService : ISpawnerEntryDataService
    {
        private readonly IEnumerable<ItemCategory> _displayedItemCategories = new[]
        {
            ItemCategory.MeatFortress,
            ItemCategory.Magazine,
            ItemCategory.Cartridge,
            ItemCategory.Clip,
            ItemCategory.Speedloader,
        };

        public SpawnerEntryData ConvertToSpawnerEntryData(ItemSpawnerId spawnerId)
        {
            return new SpawnerEntryData
            {
                Path = GetSpawnerEntryPathFromSpawnerId(spawnerId)
            };
        }

        private string GetSpawnerEntryPathFromSpawnerId(ItemSpawnerId spawnerId)
        {
            string path = GetPageForSpawnerId(spawnerId).ToString();

            if (ShouldDisplayMainCategory(spawnerId))
            {
                path += "/" + GetTagFromCategory(spawnerId.Category);
            }

            if (ShouldDisplaySubcategory(spawnerId))
            {
                path += "/" + GetTagFromSubcategory(spawnerId.SubCategory);
            }

            path += "/" + spawnerId.MainObjectId;

            return path;
        }

        private PageMode GetPageForSpawnerId(ItemSpawnerId spawnerId)
        {
            if (ShouldItemBeTaggedAsToolsToy(spawnerId))
            {
                return PageMode.ToolsToys;
            }
            else if (ShouldItemBeTaggedAsMelee(spawnerId))
            {
                return PageMode.Melee;
            }
            else if (ShouldItemBeTaggedAsAmmo(spawnerId))
            {
                return PageMode.Ammo;
            }
            else if (ShouldItemBeTaggedAsAttachment(spawnerId))
            {
                return PageMode.Attachments;
            }
            else if (ShouldItemBeTaggedAsFirearm(spawnerId))
            {
                return PageMode.Firearms;
            }

            return PageMode.MainMenu;
        }

        //TODO: spawnerIds should have it's own Core FVRObject instance
        private bool ShouldItemBeTaggedAsFirearm(ItemSpawnerId spawnerId)
        {
            return 
                spawnerId.MainObject.Category == FVRObject.ObjectCategory.Firearm ||
                _firearmItemCategories.Contains(spawnerId.Category) ||
                !Enum.IsDefined(typeof(ItemCategory), spawnerId.Category);
        }

        private bool ShouldItemBeTaggedAsToolsToy(ItemSpawnerId spawnerId)
        {
            return 
                spawnerId.MainObject.Category == FVRObject.ObjectCategory.Explosive ||
                spawnerId.MainObject.Category == FVRObject.ObjectCategory.Thrown ||
                spawnerId.SubCategory == SubCategory.Grenade ||
                spawnerId.SubCategory == SubCategory.RemoteExplosives ||
                spawnerId.Category == ItemCategory.Misc;
        }

        private bool ShouldItemBeTaggedAsMelee(ItemSpawnerId spawnerId)
        {
            return 
                spawnerId.MainObject.Category == FVRObject.ObjectCategory.MeleeWeapon ||
                spawnerId.Category == ItemCategory.Melee;
        }

        private bool ShouldItemBeTaggedAsAmmo(ItemSpawnerId spawnerId)
        {
            return 
                spawnerId.Category == ItemCategory.Magazine ||
                spawnerId.Category == ItemCategory.Clip ||
                spawnerId.Category == ItemCategory.Speedloader ||
                spawnerId.Category == ItemCategory.Cartridge;
        }

        private bool ShouldItemBeTaggedAsAttachment(ItemSpawnerId spawnerId)
        {
            return spawnerId.MainObject.Category == FVRObject.ObjectCategory.Attachment;
        }


        private bool ShouldDisplayMainCategory(ItemSpawnerId spawnerId)
        {
            var isDisplayableVanillaCategory = _displayedItemCategories.Contains(spawnerId.Category);

            var isModdedMainCategory = !Enum.IsDefined(typeof(ItemCategory), spawnerId.Category);

            return isDisplayableVanillaCategory || isModdedMainCategory;
        }

        private bool ShouldDisplaySubcategory(ItemSpawnerId spawnerId)
        {
            return spawnerId.SubCategory != SubCategory.None;
        }

        private string GetTagFromCategory(ItemCategory category)
        {
            if (Enum.IsDefined(typeof(ItemCategory), category)) return category.ToString();

            return "ModdedCategory_" + category.ToString();
        }

        private string GetTagFromSubcategory(SubCategory subcategory)
        {
            if (Enum.IsDefined(typeof(SubCategory), subcategory)) return subcategory.ToString();

            return "ModdedSubcategory_" + subcategory.ToString();
        }
    }
}
