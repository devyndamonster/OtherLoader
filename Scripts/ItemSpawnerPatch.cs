using FistVR;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OtherLoader
{
    public class ItemSpawnerPatch
    {

        [HarmonyPatch(typeof(ItemSpawnerUI), "Start")]
        [HarmonyPrefix]
        private static bool StartPatch(ItemSpawnerUI __instance)
        {
            OtherLogger.Log("Start of itemspawner", OtherLogger.LogType.General);

            __instance.gameObject.AddComponent<ItemSpawnerDataObject>();

            return true;
        }



        [HarmonyPatch(typeof(ItemSpawnerUI), "SetMode_Home")]
        [HarmonyPrefix]
        private static bool SetModeHomePatch(ItemSpawnerUI __instance)
        {
            OtherLogger.Log("Setting Mode To Home", OtherLogger.LogType.General);

            ItemSpawnerDataObject data = __instance.GetComponent<ItemSpawnerDataObject>();

            __instance.m_curMode = ItemSpawnerUI.ItemSpawnerPageMode.Home;
            __instance.P_Tiles.SetActive(true);
            __instance.P_Details.SetActive(false);
            __instance.P_Vault.SetActive(false);
            __instance.B_Spawn.SetActive(false);
            __instance.B_Back.SetActive(false);
            __instance.B_Vault.SetActive(true);
            __instance.B_Scan.SetActive(false);
            __instance.T_TopBar.text = "HOME";
            

            data.maxHomePage = GetMaxHomePage();
            if (data.currHomePage > data.maxHomePage) data.currHomePage = data.maxHomePage;

            OtherLogger.Log("Max Home Page: " + data.maxHomePage, OtherLogger.LogType.General);

            if (data.maxHomePage > 0)
            {
                if(data.currHomePage == 0)
                {
                    __instance.B_Next.SetActive(true);
                    __instance.B_Prev.SetActive(false);
                }

                else if(data.currHomePage == data.maxHomePage)
                {
                    __instance.B_Next.SetActive(false);
                    __instance.B_Prev.SetActive(true);
                }

                else
                {
                    __instance.B_Next.SetActive(true);
                    __instance.B_Prev.SetActive(true);
                }

                __instance.B_PageIndicator.SetActive(true);
            }

            else
            {
                __instance.B_Next.SetActive(false);
                __instance.B_Prev.SetActive(false);
                __instance.B_PageIndicator.SetActive(false);
            }


            __instance.Draw_Tiles_Home();
            __instance.ControlPoster.gameObject.SetActive(false);

            __instance.UpdatePageIndicator(data.currHomePage, data.maxHomePage);

            return false;
        }


        [HarmonyPatch(typeof(ItemSpawnerUI), "Draw_Tiles_Home")]
        [HarmonyPrefix]
        private static bool DrawTilesHomePatch(ItemSpawnerUI __instance)
        {
            OtherLogger.Log("Drawing Tiles For Home", OtherLogger.LogType.General);

            ItemSpawnerDataObject data = __instance.GetComponent<ItemSpawnerDataObject>();

            List<ItemSpawnerCategoryDefinitions.Category> cats = GetVisibleCategories();

            OtherLogger.Log("Num Categories = " + cats.Count, OtherLogger.LogType.General);

            int displayIndex = 0;
            for(int i = data.currHomePage * 10; i < cats.Count && displayIndex < 10; i++)
            {
                OtherLogger.Log("Loop! Display Index = " + displayIndex + ", i = " + i, OtherLogger.LogType.General);

                if(IM.CD.ContainsKey(cats[i].Cat))
                {
                    __instance.Tiles_SelectionPage[displayIndex].gameObject.SetActive(true);
                    __instance.Tiles_SelectionPage[displayIndex].Image.sprite = cats[i].Sprite;
                    __instance.Tiles_SelectionPage[displayIndex].Text.text = cats[i].DisplayName;
                    __instance.Tiles_SelectionPage[displayIndex].Category = cats[i].Cat;
                    __instance.Tiles_SelectionPage[displayIndex].LockedCorner.gameObject.SetActive(false);

                    OtherLogger.Log("Displaying Category: " + cats[i].DisplayName, OtherLogger.LogType.General);

                    displayIndex += 1;
                }
            }

            for(int i = displayIndex; i < __instance.Tiles_SelectionPage.Length; i++)
            {
                __instance.Tiles_SelectionPage[i].gameObject.SetActive(false);
            }

            return false;
        }



        [HarmonyPatch(typeof(ItemSpawnerUI), "ButtonPress_Next")]
        [HarmonyPrefix]
        private static bool ButtonPressNextPatch(ItemSpawnerUI __instance)
        {
            if (__instance.refireTick > 0f) return false;

            if(__instance.m_curMode == ItemSpawnerUI.ItemSpawnerPageMode.Home)
            {
                ItemSpawnerDataObject data = __instance.GetComponent<ItemSpawnerDataObject>();

                __instance.ButtonPress(0);
                data.currHomePage += 1;

                if (data.currHomePage == data.maxHomePage)
                {
                    __instance.B_Next.SetActive(false);
                    __instance.B_Prev.SetActive(true);
                }

                else
                {
                    __instance.B_Next.SetActive(true);
                    __instance.B_Prev.SetActive(true);
                }

                __instance.UpdatePageIndicator(data.currHomePage, data.maxHomePage);
                __instance.Draw_Tiles_Home();

                return false;
            }

            return true;
        }



        [HarmonyPatch(typeof(ItemSpawnerUI), "ButtonPress_Prev")]
        [HarmonyPrefix]
        private static bool ButtonPressPrevPatch(ItemSpawnerUI __instance)
        {
            if (__instance.refireTick > 0f) return false;

            if (__instance.m_curMode == ItemSpawnerUI.ItemSpawnerPageMode.Home)
            {
                ItemSpawnerDataObject data = __instance.GetComponent<ItemSpawnerDataObject>();

                __instance.ButtonPress(1);
                data.currHomePage -= 1;

                if (data.currHomePage == 0)
                {
                    __instance.B_Next.SetActive(true);
                    __instance.B_Prev.SetActive(false);
                }

                else
                {
                    __instance.B_Next.SetActive(true);
                    __instance.B_Prev.SetActive(true);
                }

                __instance.UpdatePageIndicator(data.currHomePage, data.maxHomePage);
                __instance.Draw_Tiles_Home();

                return false;
            }

            return true;
        }



        public static int GetMaxHomePage()
        {
            int numCats = 0;

            foreach(ItemSpawnerCategoryDefinitions.Category cat in IM.CDefs.Categories)
            {
                bool displayCategory = cat.DoesDisplay_Sandbox;
                if (GM.CurrentSceneSettings.UsesUnlockSystem) displayCategory = cat.DoesDisplay_Unlocks;

                if (displayCategory) numCats += 1;
            }

            return (numCats - 1) / 10;
        }



        public static List<ItemSpawnerCategoryDefinitions.Category> GetVisibleCategories()
        {
            List<ItemSpawnerCategoryDefinitions.Category> visible = new List<ItemSpawnerCategoryDefinitions.Category>();

            foreach (ItemSpawnerCategoryDefinitions.Category cat in IM.CDefs.Categories)
            {
                bool displayCategory = cat.DoesDisplay_Sandbox;
                if (GM.CurrentSceneSettings.UsesUnlockSystem) displayCategory = cat.DoesDisplay_Unlocks;

                if (displayCategory) visible.Add(cat);
            }

            return visible;
        }

    }


    public class ItemSpawnerDataObject : MonoBehaviour
    {

        public int currHomePage = 0;
        public int currCategoryPage = 0;

        public int maxHomePage = 0;
        public int maxCategoryPage = 0;

    }

}
