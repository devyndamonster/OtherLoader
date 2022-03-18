using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OtherLoader.Loaders
{
    public class RoundDisplayDataLoader : BaseAssetLoader
    {

        public override IEnumerator LoadAssetsFromBundle(AssetBundle assetBundle, string bundleId)
        {
            return LoadAssetsFromBundle<FVRFireArmRoundDisplayData>(assetBundle, bundleId);
        }

        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            FVRFireArmRoundDisplayData displayData = asset as FVRFireArmRoundDisplayData;

            AddRoundTypeIfNotExist(displayData);
            AddRoundClassIfNotExist(displayData);
            AddRoundDisplayData(displayData);

            OtherLogger.Log("Loaded ammo type: " + displayData.Type, OtherLogger.LogType.Loading);
        }

        private void AddRoundTypeIfNotExist(FVRFireArmRoundDisplayData displayData)
        {
            AM.STypeDic.CreateValueIfNewKey(displayData.Type);
            AM.STypeList.AddIfUnique(displayData.Type);
        }


        private void AddRoundClassIfNotExist(FVRFireArmRoundDisplayData displayData)
        {
            AM.STypeClassLists.CreateValueIfNewKey(displayData.Type);

            foreach (FVRFireArmRoundDisplayData.DisplayDataClass roundClass in displayData.Classes)
            {
                OtherLogger.Log("Loading ammo class: " + roundClass.Class, OtherLogger.LogType.Loading);
                AM.STypeDic[displayData.Type][roundClass.Class] = roundClass;
                AM.STypeClassLists[displayData.Type].AddIfUnique(roundClass.Class);
            }
        }

        private void AddRoundDisplayData(FVRFireArmRoundDisplayData displayData)
        {
            if (!AM.SRoundDisplayDataDic.ContainsKey(displayData.Type))
            {
                AM.SRoundDisplayDataDic[displayData.Type] = displayData;
            }

            //If this Display Data already exists, then we should add our classes to the existing display data class list
            else
            {
                FVRFireArmRoundDisplayData.DisplayDataClass[] currentDisplayClasses = AM.SRoundDisplayDataDic[displayData.Type].Classes;

                List<FVRFireArmRoundDisplayData.DisplayDataClass> mutableClassList = currentDisplayClasses.ToList();
                mutableClassList.AddRange(displayData.Classes);

                AM.SRoundDisplayDataDic[displayData.Type].Classes = mutableClassList.ToArray();
            }
        }

        



    }
}
