using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Services
{
    public class MetaDataService : IMetaDataService
    {
		private readonly ItemSpawnerID.EItemCategory[] _firearmItemCategories = new ItemSpawnerID.EItemCategory[]
		{
			ItemSpawnerID.EItemCategory.Pistol,
			ItemSpawnerID.EItemCategory.Shotgun,
			ItemSpawnerID.EItemCategory.SMG_Rifle,
			ItemSpawnerID.EItemCategory.Support
		};

		private readonly IPathService _pathService;

		public MetaDataService(IPathService pathService)
        {
			_pathService = pathService;
        }

		public ItemSpawnerV2.PageMode GetSpawnerPageForFVRObject(FVRObject fvrObject)
        {
			return IM.Instance.PageItemLists.FirstOrDefault(o => o.Value.Contains(fvrObject.ItemID)).Key;
		}

		private ItemSpawnerV2.PageMode GetSpawnerPageForSpawnerEntry(ItemSpawnerEntry entry)
		{
			ItemSpawnerV2.PageMode page = ItemSpawnerV2.PageMode.Firearms;
			string pageString = _pathService.GetRootPath(entry.EntryPath);

			if (Enum.IsDefined(typeof(ItemSpawnerV2.PageMode), pageString))
			{
				page = (ItemSpawnerV2.PageMode)Enum.Parse(typeof(ItemSpawnerV2.PageMode), pageString);
			}

			return page;
		}

		public ItemSpawnerV2.PageMode GetSpawnerPageForSpawnerId(ItemSpawnerID spawnerId)
        {
			if (ShouldItemBeTaggedAsToolsToy(spawnerId))
			{
				return ItemSpawnerV2.PageMode.ToolsToys;
			}
			else if (ShouldItemBeTaggedAsMelee(spawnerId))
			{
				return ItemSpawnerV2.PageMode.Melee;
			}
			else if (ShouldItemBeTaggedAsAmmo(spawnerId))
            {
				return ItemSpawnerV2.PageMode.Ammo;
			}
			else if (ShouldItemBeTaggedAsAttachment(spawnerId))
            {
				return ItemSpawnerV2.PageMode.Attachments;
            }
			else if (ShouldItemBeTaggedAsFirearm(spawnerId))
			{
				return ItemSpawnerV2.PageMode.Firearms;
			}

			return ItemSpawnerV2.PageMode.MainMenu;
		}

        public string GetTagFromCategory(ItemSpawnerID.EItemCategory category)
        {
            if (Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), category)) return category.ToString();
            
            return "ModdedCategory_" + category.ToString();
        }

        public string GetTagFromSubcategory(ItemSpawnerID.ESubCategory subcategory)
        {
			if (Enum.IsDefined(typeof(ItemSpawnerID.ESubCategory), subcategory)) return subcategory.ToString();

			return "ModdedSubcategory_" + subcategory.ToString();
		}

        public void RegisterSpawnerIDIntoTagSystem(ItemSpawnerID spawnerID)
        {
			OtherLogger.Log($"Attempting to tag {spawnerID.MainObject.ItemID}", OtherLogger.LogType.Loading);

			var page = GetSpawnerPageForSpawnerId(spawnerID);

			RegisterModTags(spawnerID, page);
			RegisterCategoryTags(spawnerID, page);
			RegisterNewTag(spawnerID.MainObject, page);

			if (page == ItemSpawnerV2.PageMode.Firearms)
            {
				RegisterFirearmIntoMetaTagSystem(spawnerID.MainObject, page);
            }
			else if (page == ItemSpawnerV2.PageMode.Attachments)
            {
				RegisterAttachmentIntoMetaTagSystem(spawnerID.MainObject, page);
            }
			else if (page == ItemSpawnerV2.PageMode.Ammo)
            {
				RegisterAmmoIntoMetaTagSystem(spawnerID.MainObject, page);
            }
		}

		public void RegisterSpawnerEntryIntoTagSystem(ItemSpawnerEntry spawnerEntry)
		{
			var page = GetSpawnerPageForSpawnerEntry(spawnerEntry);
			var mainObject = IM.OD[spawnerEntry.MainObjectID];

			RegisterModTags(spawnerEntry, page);
			RegisterCategoryTags(spawnerEntry, page);
			RegisterNewTag(mainObject, page);
		}

		private void RegisterCategoryTags(ItemSpawnerID spawnerId, ItemSpawnerV2.PageMode page)
		{
			IM.AddMetaTag(GetTagFromCategory(spawnerId.Category), TagType.Category, spawnerId.MainObject.ItemID, page);
			IM.AddMetaTag(GetTagFromSubcategory(spawnerId.SubCategory), TagType.SubCategory, spawnerId.MainObject.ItemID, page);
		}

		private void RegisterCategoryTags(ItemSpawnerEntry spawnerEntry, ItemSpawnerV2.PageMode page)
		{
			var category = spawnerEntry.GetSpawnerCategory();
			var subcategory = spawnerEntry.GetSpawnerSubcategory();

            if (category.HasValue)
            {
				IM.AddMetaTag(category.Value.ToString(), TagType.Category, spawnerEntry.MainObjectID, page);
			}

			if (subcategory.HasValue)
			{
				IM.AddMetaTag(subcategory.Value.ToString(), TagType.SubCategory, spawnerEntry.MainObjectID, page);
			}
		}

		private void RegisterModTags(ItemSpawnerID spawnerId, ItemSpawnerV2.PageMode page)
		{
			foreach (string tag in spawnerId.ModTags)
			{
				IM.AddMetaTag(tag, TagType.ModTag, spawnerId.MainObject.ItemID, page);
			}

			if (spawnerId.MainObject.IsModContent)
			{
				IM.AddMetaTag("Mod Content", TagType.Set, spawnerId.MainObject.ItemID, page);
			}
		}

		private void RegisterModTags(ItemSpawnerEntry spawnerEntry, ItemSpawnerV2.PageMode page)
		{
			foreach (string tag in spawnerEntry.ModTags)
			{
				IM.AddMetaTag(tag, TagType.ModTag, spawnerEntry.MainObjectID, page);
			}

			if (spawnerEntry.IsModded)
			{
				IM.AddMetaTag("Mod Content", TagType.Set, spawnerEntry.MainObjectID, page);
			}
		}

		private void RegisterNewTag(FVRObject mainObject, ItemSpawnerV2.PageMode page)
        {
			foreach(var group in ManagerSingleton<IM>.Instance.NewItems.Groups)
            {
                if (group.NewItemIDs.Contains(mainObject.ItemID))
                {
					IM.AddMetaTag(group.Tag, TagType.New, mainObject.ItemID, page);
				}
            }
		}

		private void RegisterFirearmIntoMetaTagSystem(FVRObject mainObject, ItemSpawnerV2.PageMode page)
		{
			IM.AddMetaTag(mainObject.TagSet.ToString(), TagType.Set, mainObject.ItemID, page);
			IM.AddMetaTag(mainObject.TagEra.ToString(), TagType.Era, mainObject.ItemID, page);
			IM.AddMetaTag(mainObject.TagFirearmSize.ToString(), TagType.Size, mainObject.ItemID, page);
			IM.AddMetaTag(mainObject.TagFirearmAction.ToString(), TagType.Action, mainObject.ItemID, page);
			IM.AddMetaTag(mainObject.TagFirearmRoundPower.ToString(), TagType.RoundClass, mainObject.ItemID, page);
			IM.AddMetaTag(mainObject.TagFirearmCountryOfOrigin.ToString(), TagType.CountryOfOrigin, mainObject.ItemID, page);
			IM.AddMetaTag(mainObject.TagFirearmFirstYear.ToString(), TagType.IntroductionYear, mainObject.ItemID, page);
			IM.AddMetaTag(mainObject.MagazineType.ToString(), TagType.MagazineType, mainObject.ItemID, page);

			if (mainObject.UsesRoundTypeFlag)
				IM.AddMetaTag(mainObject.RoundType.ToString(), TagType.Caliber, mainObject.ItemID, page);

			mainObject.TagFirearmFiringModes.ForEach(tag =>
				IM.AddMetaTag(tag.ToString(), TagType.FiringMode, mainObject.ItemID, page));

			mainObject.TagFirearmFeedOption.ForEach(tag =>
				IM.AddMetaTag(tag.ToString(), TagType.FeedOption, mainObject.ItemID, page));

			mainObject.TagFirearmMounts.ForEach(mode =>
				IM.AddMetaTag(mode.ToString(), TagType.AttachmentMount, mainObject.ItemID, page));
		}

		private void RegisterAttachmentIntoMetaTagSystem(FVRObject mainObject, ItemSpawnerV2.PageMode page)
		{
			IM.AddMetaTag(mainObject.TagSet.ToString(), TagType.Set, mainObject.ItemID, page);
			IM.AddMetaTag(mainObject.TagEra.ToString(), TagType.Era, mainObject.ItemID, page);
			IM.AddMetaTag(mainObject.TagAttachmentFeature.ToString(), TagType.AttachmentFeature, mainObject.ItemID, page);
			IM.AddMetaTag(mainObject.TagAttachmentMount.ToString(), TagType.AttachmentMount, mainObject.ItemID, page);
		}

		private void RegisterAmmoIntoMetaTagSystem(FVRObject mainObject, ItemSpawnerV2.PageMode page)
		{
			IM.AddMetaTag(mainObject.TagSet.ToString(), TagType.Set, mainObject.ItemID, page);
			IM.AddMetaTag(mainObject.TagEra.ToString(), TagType.Era, mainObject.ItemID, page);
			IM.AddMetaTag(mainObject.MagazineType.ToString(), TagType.MagazineType, mainObject.ItemID, page);

			if (mainObject.UsesRoundTypeFlag)
            {
				IM.AddMetaTag(mainObject.RoundType.ToString(), TagType.Caliber, mainObject.ItemID, page);
			}
		}

		private bool ShouldItemBeTaggedAsFirearm(ItemSpawnerID spawnerId)
        {
			return spawnerId.MainObject.Category == FVRObject.ObjectCategory.Firearm ||
				_firearmItemCategories.Contains(spawnerId.Category) ||
				!Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), spawnerId.Category);
		}

		private bool ShouldItemBeTaggedAsToolsToy(ItemSpawnerID spawnerId)
        {
			return spawnerId.MainObject.Category == FVRObject.ObjectCategory.Explosive ||
				spawnerId.MainObject.Category == FVRObject.ObjectCategory.Thrown ||
				spawnerId.SubCategory == ItemSpawnerID.ESubCategory.Grenade ||
				spawnerId.SubCategory == ItemSpawnerID.ESubCategory.RemoteExplosives ||
				spawnerId.Category == ItemSpawnerID.EItemCategory.Misc;
		}

		private bool ShouldItemBeTaggedAsMelee(ItemSpawnerID spawnerId)
        {
			return spawnerId.MainObject.Category == FVRObject.ObjectCategory.MeleeWeapon ||
				spawnerId.Category == ItemSpawnerID.EItemCategory.Melee;
		}

		private bool ShouldItemBeTaggedAsAmmo(ItemSpawnerID spawnerId)
        {
			return spawnerId.Category == ItemSpawnerID.EItemCategory.Magazine ||
				spawnerId.Category == ItemSpawnerID.EItemCategory.Clip ||
				spawnerId.Category == ItemSpawnerID.EItemCategory.Speedloader ||
				spawnerId.Category == ItemSpawnerID.EItemCategory.Cartridge;
		} 

		private bool ShouldItemBeTaggedAsAttachment(ItemSpawnerID spawnerId)
        {
			return spawnerId.MainObject.Category == FVRObject.ObjectCategory.Attachment;
		}
    }
}
