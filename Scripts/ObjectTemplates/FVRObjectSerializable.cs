using Anvil;
using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader
{
    public class FVRObjectSerializable
    {

        public string ItemID;
        public string DisplayName;
        public FVRObject.ObjectCategory Category;
        public string SpawnedFromID;
        public float Mass;
        public int MagazineCapacity;
        public bool RequiresPicatinnySight;
        public FVRObject.OTagEra TagEra;
		public FVRObject.OTagSet TagSet;
		public FVRObject.OTagFirearmSize TagFirearmSize;
		public FVRObject.OTagFirearmAction TagFirearmAction;
		public FVRObject.OTagFirearmRoundPower TagFirearmRoundPower;
		public FVRObject.OTagFirearmCountryOfOrigin TagFirearmCountryOfOrigin;
		public int TagFirearmFirstYear;
		public List<FVRObject.OTagFirearmFiringMode> TagFirearmFiringModes;
		public List<FVRObject.OTagFirearmFeedOption> TagFirearmFeedOption;
		public List<FVRObject.OTagFirearmMount> TagFirearmMounts;
		public FVRObject.OTagFirearmMount TagAttachmentMount;
		public FVRObject.OTagAttachmentFeature TagAttachmentFeature;
		public FVRObject.OTagMeleeStyle TagMeleeStyle;
		public FVRObject.OTagMeleeHandedness TagMeleeHandedness;
		public FVRObject.OTagPowerupType TagPowerupType;
		public FVRObject.OTagThrownType TagThrownType;
		public FVRObject.OTagThrownDamageType TagThrownDamageType;
		public FireArmMagazineType MagazineType;
		public List<string> CompatibleMagazines;
		public List<string> CompatibleClips;
		public List<string> CompatibleSpeedLoaders;
		public List<string> CompatibleSingleRounds;
		public List<string> BespokeAttachments;
		public List<string> RequiredSecondaryPieces;
		public int MinCapacityRelated = -1;
		public int MaxCapacityRelated = -1;
		public int CreditCost;
		public bool OSple = true;
		public AssetID AnvilPrefab;
		public bool Export = true;

		public FVRObjectSerializable() { }

		public FVRObjectSerializable(FVRObject fvr)
        {
			ItemID = fvr.ItemID;
			DisplayName = fvr.DisplayName;
			Category = fvr.Category;
			SpawnedFromID = fvr.SpawnedFromId;
			Mass = fvr.Mass;
			MagazineCapacity = fvr.MagazineCapacity;
			RequiresPicatinnySight = fvr.RequiresPicatinnySight;
			TagEra = fvr.TagEra;
			TagSet = fvr.TagSet;
			TagFirearmSize = fvr.TagFirearmSize;
			TagFirearmAction = fvr.TagFirearmAction;
			TagFirearmRoundPower = fvr.TagFirearmRoundPower;
			TagFirearmCountryOfOrigin = fvr.TagFirearmCountryOfOrigin;
			TagFirearmFirstYear = fvr.TagFirearmFirstYear;
			TagFirearmFiringModes = fvr.TagFirearmFiringModes;
			TagFirearmFeedOption = fvr.TagFirearmFeedOption;
			TagFirearmMounts = fvr.TagFirearmMounts;
			TagAttachmentMount = fvr.TagAttachmentMount;
			TagAttachmentFeature = fvr.TagAttachmentFeature;
			TagMeleeStyle = fvr.TagMeleeStyle;
			TagMeleeHandedness = fvr.TagMeleeHandedness;
			TagPowerupType = fvr.TagPowerupType;
			TagThrownType = fvr.TagThrownType;
			TagThrownDamageType = fvr.TagThrownDamageType;
			MagazineType = fvr.MagazineType;
			CompatibleMagazines = fvr.CompatibleMagazines.Select(o => o.ItemID).ToList();
			CompatibleClips = fvr.CompatibleClips.Select(o => o.ItemID).ToList();
			CompatibleSpeedLoaders = fvr.CompatibleSpeedLoaders.Select(o => o.ItemID).ToList();
			CompatibleSingleRounds = fvr.CompatibleSingleRounds.Select(o => o.ItemID).ToList();
			BespokeAttachments = fvr.BespokeAttachments.Select(o => o.ItemID).ToList();
			RequiredSecondaryPieces = fvr.RequiredSecondaryPieces.Select(o => o.ItemID).ToList();
			MinCapacityRelated = fvr.MinCapacityRelated;
			MaxCapacityRelated = fvr.MaxCapacityRelated;
			CreditCost = fvr.CreditCost;
			OSple = fvr.OSple;
			AnvilPrefab = fvr.m_anvilPrefab;
			Export = fvr.m_export;
		}

		public FVRObject GetFVRObject()
        {
			FVRObject fvr = new FVRObject();

			fvr.ItemID = ItemID;
			fvr.DisplayName = DisplayName;
			fvr.Category = Category;
			fvr.SpawnedFromId = SpawnedFromID;
			fvr.Mass = Mass;
			fvr.MagazineCapacity = MagazineCapacity;
			fvr.RequiresPicatinnySight = RequiresPicatinnySight;
			fvr.TagEra = TagEra;
			fvr.TagSet = TagSet;
			fvr.TagFirearmSize = TagFirearmSize;
			fvr.TagFirearmAction = TagFirearmAction;
			fvr.TagFirearmRoundPower = TagFirearmRoundPower;
			fvr.TagFirearmCountryOfOrigin = TagFirearmCountryOfOrigin;
			fvr.TagFirearmFirstYear = TagFirearmFirstYear;
			fvr.TagFirearmFiringModes = TagFirearmFiringModes;
			fvr.TagFirearmFeedOption = TagFirearmFeedOption;
			fvr.TagFirearmMounts = TagFirearmMounts;
			fvr.TagAttachmentMount = TagAttachmentMount;
			fvr.TagAttachmentFeature = TagAttachmentFeature;
			fvr.TagMeleeStyle = TagMeleeStyle;
			fvr.TagMeleeHandedness = TagMeleeHandedness;
			fvr.TagPowerupType = TagPowerupType;
			fvr.TagThrownType = TagThrownType;
			fvr.TagThrownDamageType = TagThrownDamageType;
			fvr.MagazineType = MagazineType;
			fvr.CompatibleMagazines = CompatibleMagazines.Select(o => IM.OD[o]).ToList();
			fvr.CompatibleClips = CompatibleClips.Select(o => IM.OD[o]).ToList();
			fvr.CompatibleSpeedLoaders = CompatibleSpeedLoaders.Select(o => IM.OD[o]).ToList();
			fvr.CompatibleSingleRounds = CompatibleSingleRounds.Select(o => IM.OD[o]).ToList();
			fvr.BespokeAttachments = BespokeAttachments.Select(o => IM.OD[o]).ToList();
			fvr.RequiredSecondaryPieces = RequiredSecondaryPieces.Select(o => IM.OD[o]).ToList();
			fvr.MinCapacityRelated = MinCapacityRelated;
			fvr.MaxCapacityRelated = MaxCapacityRelated;
			fvr.CreditCost = CreditCost;
			fvr.OSple = OSple;
			fvr.m_anvilPrefab = AnvilPrefab;
			fvr.m_export = Export;

			return fvr;
        }

	}
}
