using FistVR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OtherLoader.Loaders
{
    public class RoundDisplayDataLoader : BaseAssetLoader
    {
        protected override void LoadAsset(UnityEngine.Object asset, string bundleId)
        {
            FVRFireArmRoundDisplayData displayData = asset as FVRFireArmRoundDisplayData;


            TryAddNewRoundType(displayData);
            TryAddNewRoundClassList(displayData);
            TryAddToSRoundDisplayDataDic(displayData);








            foreach (FVRFireArmRoundDisplayData.DisplayDataClass roundClass in data.Classes)
            {
                OtherLogger.Log("Loading ammo class: " + roundClass.Class, OtherLogger.LogType.Loading);
                if (!AM.STypeDic[data.Type].ContainsKey(roundClass.Class))
                {
                    OtherLogger.Log("This is a new ammo class! Adding it to dictionary", OtherLogger.LogType.Loading);
                    AM.STypeDic[data.Type].Add(roundClass.Class, roundClass);
                }
                else
                {
                    OtherLogger.LogError("Ammo class already exists for bullet type! Bullet will not be loaded! Type: " + data.Type + ", Class: " + roundClass.Class);
                    return;
                }

                if (!AM.STypeClassLists[data.Type].Contains(roundClass.Class))
                {
                    AM.STypeClassLists[data.Type].Add(roundClass.Class);
                }
            }

            OtherLogger.Log("Loaded ammo type: " + displayData.Type, OtherLogger.LogType.Loading);
        }

        private void TryAddNewRoundType(FVRFireArmRoundDisplayData displayData)
        {
            if (!AM.STypeDic.ContainsKey(displayData.Type))
            {
                OtherLogger.Log("This is a new ammo type! Adding it to dictionary", OtherLogger.LogType.Loading);
                AM.STypeDic.Add(displayData.Type, new Dictionary<FireArmRoundClass, FVRFireArmRoundDisplayData.DisplayDataClass>());
            }

            if (!AM.STypeList.Contains(displayData.Type))
            {
                AM.STypeList.Add(displayData.Type);
            }
        }

        private void TryAddNewRoundClassList(FVRFireArmRoundDisplayData displayData)
        {
            if (!AM.STypeClassLists.ContainsKey(displayData.Type))
            {
                AM.STypeClassLists.Add(displayData.Type, new List<FireArmRoundClass>());
            }
        }

        private void TryAddToSRoundDisplayDataDic(FVRFireArmRoundDisplayData displayData)
        {
            if (!AM.SRoundDisplayDataDic.ContainsKey(displayData.Type))
            {
                AM.SRoundDisplayDataDic.Add(displayData.Type, displayData);
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
