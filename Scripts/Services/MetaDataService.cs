using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Services
{
    public class MetaDataService : IMetaDataService
    {
        public ItemSpawnerV2.PageMode GetSpawnerPageForSpawnerId(ItemSpawnerID spawnerId)
        {
			return IM.Instance.PageItemLists.FirstOrDefault(o => o.Value.Contains(spawnerId.ItemID)).Key;
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
			if (spawnerID.Category == ItemSpawnerID.EItemCategory.MeatFortress)
			{
				if (spawnerID.MainObject.Category == FVRObject.ObjectCategory.Firearm)
				{
					TagFirearm(spawnerID);
				}
				else if (spawnerID.MainObject.Category == FVRObject.ObjectCategory.Explosive || spawnerID.MainObject.Category == FVRObject.ObjectCategory.Thrown)
				{
					TagMisc(spawnerID, ItemSpawnerV2.PageMode.ToolsToys);
				}
				else if (spawnerID.MainObject.Category == FVRObject.ObjectCategory.MeleeWeapon)
				{
					TagMisc(spawnerID, ItemSpawnerV2.PageMode.Melee);
				}
				else if (spawnerID.MainObject.Category == FVRObject.ObjectCategory.Attachment)
				{
					TagAttachment(spawnerID);
				}
				OtherLogger.Log($"Reason for page selection: Category = {spawnerID.Category} & ObjectCategory = {spawnerID.MainObject.Category}", OtherLogger.LogType.Loading);
			}
			else if (spawnerID.MainObject.Category == FVRObject.ObjectCategory.Firearm)
			{
				TagFirearm(spawnerID);
				OtherLogger.Log($"Reason for page selection: ObjectCategory = {spawnerID.MainObject.Category}", OtherLogger.LogType.Loading);
			}
			else if (spawnerID.Category == ItemSpawnerID.EItemCategory.Magazine || spawnerID.Category == ItemSpawnerID.EItemCategory.Clip || spawnerID.Category == ItemSpawnerID.EItemCategory.Speedloader || spawnerID.Category == ItemSpawnerID.EItemCategory.Cartridge)
			{
				TagAmmo(spawnerID);
				OtherLogger.Log($"Reason for page selection: Category = {spawnerID.Category}", OtherLogger.LogType.Loading);
			}
			else if (spawnerID.Category == ItemSpawnerID.EItemCategory.Melee)
			{
				TagMisc(spawnerID, ItemSpawnerV2.PageMode.Melee);
				OtherLogger.Log($"Reason for page selection: Category = {spawnerID.Category}", OtherLogger.LogType.Loading);
			}
			else if (spawnerID.MainObject.Category == FVRObject.ObjectCategory.Attachment)
			{
				TagAttachment(spawnerID);
				OtherLogger.Log($"Reason for page selection: ObjectCategory = {spawnerID.MainObject.Category}", OtherLogger.LogType.Loading);
			}
			else if (spawnerID.SubCategory == ItemSpawnerID.ESubCategory.Grenade || spawnerID.SubCategory == ItemSpawnerID.ESubCategory.RemoteExplosives || spawnerID.Category == ItemSpawnerID.EItemCategory.Misc)
			{
				TagMisc(spawnerID, ItemSpawnerV2.PageMode.ToolsToys);
				OtherLogger.Log($"Reason for page selection: SubCategory = Grenade or  SubCategory = RemoteExplosives or Category = Misc", OtherLogger.LogType.Loading);
			}
			else if (!Enum.IsDefined(typeof(ItemSpawnerID.EItemCategory), spawnerID.Category))
			{
				OtherLogger.Log("Could not tag item, but it has a custom category, so we'll put it in firearms", OtherLogger.LogType.Loading);
				TagMisc(spawnerID, ItemSpawnerV2.PageMode.Firearms);
			}
			else
			{
				OtherLogger.Log("We didn't tag the item at all!", OtherLogger.LogType.Loading);
			}
		}

		private void TagFirearm(ItemSpawnerID spawnerID)
		{
			OtherLogger.Log("Tagging for page Firearms!", OtherLogger.LogType.Loading);
			IM.AddMetaTag(spawnerID.Category.ToString(), TagType.Category, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			IM.AddMetaTag(spawnerID.SubCategory.ToString(), TagType.SubCategory, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			IM.AddMetaTag(spawnerID.MainObject.TagSet.ToString(), TagType.Set, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			IM.AddMetaTag(spawnerID.MainObject.TagFirearmSize.ToString(), TagType.Size, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			IM.AddMetaTag(spawnerID.MainObject.TagEra.ToString(), TagType.Era, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			IM.AddMetaTag(spawnerID.MainObject.TagFirearmAction.ToString(), TagType.Action, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			IM.AddMetaTag(spawnerID.MainObject.TagFirearmRoundPower.ToString(), TagType.RoundClass, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			if (spawnerID.MainObject.UsesRoundTypeFlag)
			{
				IM.AddMetaTag(spawnerID.MainObject.RoundType.ToString(), TagType.Caliber, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			}
			IM.AddMetaTag(spawnerID.MainObject.TagFirearmCountryOfOrigin.ToString(), TagType.CountryOfOrigin, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			IM.AddMetaTag(spawnerID.MainObject.TagFirearmFirstYear.ToString(), TagType.IntroductionYear, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			for (int num6 = 0; num6 < spawnerID.MainObject.TagFirearmFiringModes.Count; num6++)
			{
				IM.AddMetaTag(spawnerID.MainObject.TagFirearmFiringModes[num6].ToString(), TagType.FiringMode, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			}
			for (int num7 = 0; num7 < spawnerID.MainObject.TagFirearmFeedOption.Count; num7++)
			{
				IM.AddMetaTag(spawnerID.MainObject.TagFirearmFeedOption[num7].ToString(), TagType.FeedOption, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			}
			IM.AddMetaTag(spawnerID.MainObject.MagazineType.ToString(), TagType.MagazineType, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			for (int num8 = 0; num8 < spawnerID.MainObject.TagFirearmMounts.Count; num8++)
			{
				IM.AddMetaTag(spawnerID.MainObject.TagFirearmMounts[num8].ToString(), TagType.AttachmentMount, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			}
			for (int num9 = 0; num9 < spawnerID.ModTags.Count; num9++)
			{
				IM.AddMetaTag(spawnerID.ModTags[num9].ToString(), TagType.ModTag, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			}
			if (spawnerID.MainObject.IsModContent)
			{
				IM.AddMetaTag("Mod Content", TagType.Set, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
			}
			for (int num10 = 0; num10 < ManagerSingleton<IM>.Instance.NewItems.Groups.Count; num10++)
			{
				if (ManagerSingleton<IM>.Instance.NewItems.Groups[num10].NewItemIDs.Contains(spawnerID.ItemID))
				{
					IM.AddMetaTag(ManagerSingleton<IM>.Instance.NewItems.Groups[num10].Tag, TagType.New, spawnerID.ItemID, ItemSpawnerV2.PageMode.Firearms);
				}
			}
		}


		private void TagMisc(ItemSpawnerID spawnerID, ItemSpawnerV2.PageMode page)
		{
			OtherLogger.Log("Tagging misc for page " + page, OtherLogger.LogType.Loading);
			IM.AddMetaTag(spawnerID.Category.ToString(), TagType.Category, spawnerID.ItemID, page);
			IM.AddMetaTag(spawnerID.SubCategory.ToString(), TagType.SubCategory, spawnerID.ItemID, page);
			for (int num17 = 0; num17 < spawnerID.ModTags.Count; num17++)
			{
				IM.AddMetaTag(spawnerID.ModTags[num17].ToString(), TagType.ModTag, spawnerID.ItemID, page);
			}
			if (spawnerID.MainObject.IsModContent)
			{
				IM.AddMetaTag("Mod Content", TagType.Set, spawnerID.ItemID, page);
			}
			for (int num18 = 0; num18 < ManagerSingleton<IM>.Instance.NewItems.Groups.Count; num18++)
			{
				if (ManagerSingleton<IM>.Instance.NewItems.Groups[num18].NewItemIDs.Contains(spawnerID.ItemID))
				{
					IM.AddMetaTag(ManagerSingleton<IM>.Instance.NewItems.Groups[num18].Tag, TagType.New, spawnerID.ItemID, page);
				}
			}
		}

		private void TagAttachment(ItemSpawnerID spawnerID)
		{
			OtherLogger.Log("Tagging for page Attachments!", OtherLogger.LogType.Loading);
			IM.AddMetaTag(spawnerID.SubCategory.ToString(), TagType.SubCategory, spawnerID.ItemID, ItemSpawnerV2.PageMode.Attachments);
			IM.AddMetaTag(spawnerID.MainObject.TagAttachmentFeature.ToString(), TagType.AttachmentFeature, spawnerID.ItemID, ItemSpawnerV2.PageMode.Attachments);
			IM.AddMetaTag(spawnerID.MainObject.TagAttachmentMount.ToString(), TagType.AttachmentMount, spawnerID.ItemID, ItemSpawnerV2.PageMode.Attachments);
			for (int num4 = 0; num4 < spawnerID.ModTags.Count; num4++)
			{
				IM.AddMetaTag(spawnerID.ModTags[num4].ToString(), TagType.ModTag, spawnerID.ItemID, ItemSpawnerV2.PageMode.Attachments);
			}
			if (spawnerID.MainObject.IsModContent)
			{
				IM.AddMetaTag("Mod Content", TagType.Set, spawnerID.ItemID, ItemSpawnerV2.PageMode.Attachments);
			}
			for (int num5 = 0; num5 < ManagerSingleton<IM>.Instance.NewItems.Groups.Count; num5++)
			{
				if (ManagerSingleton<IM>.Instance.NewItems.Groups[num5].NewItemIDs.Contains(spawnerID.ItemID))
				{
					IM.AddMetaTag(ManagerSingleton<IM>.Instance.NewItems.Groups[num5].Tag, TagType.New, spawnerID.ItemID, ItemSpawnerV2.PageMode.Attachments);
				}
			}
		}

		private void TagAmmo(ItemSpawnerID spawnerID)
		{
			OtherLogger.Log("Tagging for page Ammo!", OtherLogger.LogType.Loading);
			IM.AddMetaTag(spawnerID.Category.ToString(), TagType.Category, spawnerID.ItemID, ItemSpawnerV2.PageMode.Ammo);
			IM.AddMetaTag(spawnerID.MainObject.TagSet.ToString(), TagType.Set, spawnerID.ItemID, ItemSpawnerV2.PageMode.Ammo);
			if (spawnerID.MainObject.UsesRoundTypeFlag)
			{
				IM.AddMetaTag(spawnerID.MainObject.RoundType.ToString(), TagType.Caliber, spawnerID.ItemID, ItemSpawnerV2.PageMode.Ammo);
			}
			IM.AddMetaTag(spawnerID.MainObject.MagazineType.ToString(), TagType.MagazineType, spawnerID.ItemID, ItemSpawnerV2.PageMode.Ammo);
			for (int num11 = 0; num11 < spawnerID.ModTags.Count; num11++)
			{
				IM.AddMetaTag(spawnerID.ModTags[num11].ToString(), TagType.ModTag, spawnerID.ItemID, ItemSpawnerV2.PageMode.Ammo);
			}
			if (spawnerID.MainObject.IsModContent)
			{
				IM.AddMetaTag("Mod Content", TagType.Set, spawnerID.ItemID, ItemSpawnerV2.PageMode.Ammo);
			}
			for (int num12 = 0; num12 < ManagerSingleton<IM>.Instance.NewItems.Groups.Count; num12++)
			{
				if (ManagerSingleton<IM>.Instance.NewItems.Groups[num12].NewItemIDs.Contains(spawnerID.ItemID))
				{
					IM.AddMetaTag(ManagerSingleton<IM>.Instance.NewItems.Groups[num12].Tag, TagType.New, spawnerID.ItemID, ItemSpawnerV2.PageMode.Ammo);
				}
			}
		}
	}
}
