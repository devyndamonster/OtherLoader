using FistVR;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace OtherLoader
{
    public class ItemSpawnerPatch
    {

        [HarmonyPatch(typeof(ItemSpawnerUI), "Start")]
        [HarmonyPrefix]
        private static bool StartPatch(ItemSpawnerUI __instance)
        {
            __instance.gameObject.AddComponent<ItemSpawnerDataObject>();

            return true;
        }



        [HarmonyPatch(typeof(ItemSpawnerUI), "SetMode_Home")]
        [HarmonyPrefix]
        private static bool SetModeHomePatch(ItemSpawnerUI __instance)
        {
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


        [HarmonyPatch(typeof(ItemSpawnerUI), "SetMode_Category")]
        [HarmonyPrefix]
        private static bool SetModeCategoryPatch(ItemSpawnerUI __instance)
        {
            ItemSpawnerDataObject data = __instance.GetComponent<ItemSpawnerDataObject>();

            __instance.m_curMode = ItemSpawnerUI.ItemSpawnerPageMode.Category;
            __instance.P_Tiles.SetActive(true);
            __instance.P_Details.SetActive(false);
            __instance.P_Vault.SetActive(false);
            __instance.B_Spawn.SetActive(false);
            __instance.B_Back.SetActive(true);
            __instance.B_Vault.SetActive(true);
            __instance.B_Scan.SetActive(false);
            __instance.T_TopBar.text = "HOME | " + IM.CDefInfo[__instance.m_curCategory].DisplayName;


            data.maxCategoryPage = GetMaxCategoryPage(IM.CDefInfo[__instance.m_curCategory]);
            if (data.currCategoryPage > data.maxCategoryPage) data.currCategoryPage = data.maxCategoryPage;


            if (data.maxCategoryPage > 0)
            {
                if (data.currCategoryPage == 0)
                {
                    __instance.B_Next.SetActive(true);
                    __instance.B_Prev.SetActive(false);
                }

                else if (data.currCategoryPage == data.maxCategoryPage)
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


            __instance.Draw_Tiles_Category(__instance.m_curCategory);
            __instance.ControlPoster.gameObject.SetActive(false);

            __instance.UpdatePageIndicator(data.currCategoryPage, data.maxCategoryPage);

            return false;
        }


        [HarmonyPatch(typeof(ItemSpawnerUI), "Draw_Tiles_Home")]
        [HarmonyPrefix]
        private static bool DrawTilesHomePatch(ItemSpawnerUI __instance)
        {
            ItemSpawnerDataObject data = __instance.GetComponent<ItemSpawnerDataObject>();

            List<ItemSpawnerCategoryDefinitions.Category> cats = GetVisibleCategories();


            int displayIndex = 0;
            for(int i = data.currHomePage * 10; i < cats.Count && displayIndex < 10; i++)
            {

                if (IM.CD.ContainsKey(IM.CDefs.Categories[i].Cat))
                {

                    __instance.Tiles_SelectionPage[displayIndex].gameObject.SetActive(true);
                    __instance.Tiles_SelectionPage[displayIndex].Image.sprite = cats[i].Sprite;
                    __instance.Tiles_SelectionPage[displayIndex].Text.text = cats[i].DisplayName;
                    __instance.Tiles_SelectionPage[displayIndex].Category = cats[i].Cat;
                    __instance.Tiles_SelectionPage[displayIndex].LockedCorner.gameObject.SetActive(false);


                    displayIndex += 1;
                }
            }

            for(int i = displayIndex; i < __instance.Tiles_SelectionPage.Length; i++)
            {
                __instance.Tiles_SelectionPage[i].gameObject.SetActive(false);
            }

            return false;
        }



        [HarmonyPatch(typeof(ItemSpawnerUI), "Draw_Tiles_Category")]
        [HarmonyPrefix]
        private static bool DrawTilesCategoryPatch(ItemSpawnerID.EItemCategory Category, ItemSpawnerUI __instance)
        {
            ItemSpawnerDataObject data = __instance.GetComponent<ItemSpawnerDataObject>();

            List<ItemSpawnerCategoryDefinitions.SubCategory> subs = GetVisibleSubCategories(IM.CDefInfo[Category]);


            int displayIndex = 0;
            for (int i = data.currCategoryPage * 10; i < subs.Count && displayIndex < 10; i++)
            {

                if (IM.SCD.ContainsKey(IM.CDefSubs[Category][i].Subcat))
                {

                    __instance.Tiles_SelectionPage[displayIndex].gameObject.SetActive(true);
                    __instance.Tiles_SelectionPage[displayIndex].Image.sprite = subs[i].Sprite;
                    __instance.Tiles_SelectionPage[displayIndex].Text.text = subs[i].DisplayName;
                    __instance.Tiles_SelectionPage[displayIndex].SubCategory = subs[i].Subcat;
                    __instance.Tiles_SelectionPage[displayIndex].LockedCorner.gameObject.SetActive(false);

                    displayIndex += 1;
                }

                
            }

            for (int i = displayIndex; i < __instance.Tiles_SelectionPage.Length; i++)
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

            else if (__instance.m_curMode == ItemSpawnerUI.ItemSpawnerPageMode.Category)
            {
                ItemSpawnerDataObject data = __instance.GetComponent<ItemSpawnerDataObject>();

                __instance.ButtonPress(0);
                data.currCategoryPage += 1;

                if (data.currCategoryPage == data.maxCategoryPage)
                {
                    __instance.B_Next.SetActive(false);
                    __instance.B_Prev.SetActive(true);
                }

                else
                {
                    __instance.B_Next.SetActive(true);
                    __instance.B_Prev.SetActive(true);
                }

                __instance.UpdatePageIndicator(data.currCategoryPage, data.maxCategoryPage);
                __instance.Draw_Tiles_Category(__instance.m_curCategory);

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

            else if (__instance.m_curMode == ItemSpawnerUI.ItemSpawnerPageMode.Category)
            {
                ItemSpawnerDataObject data = __instance.GetComponent<ItemSpawnerDataObject>();

                __instance.ButtonPress(1);
                data.currCategoryPage -= 1;

                if (data.currCategoryPage == 0)
                {
                    __instance.B_Next.SetActive(true);
                    __instance.B_Prev.SetActive(false);
                }

                else
                {
                    __instance.B_Next.SetActive(true);
                    __instance.B_Prev.SetActive(true);
                }

                __instance.UpdatePageIndicator(data.currCategoryPage, data.maxCategoryPage);
                __instance.Draw_Tiles_Category(__instance.m_curCategory);

                return false;
            }

            return true;
        }


        [HarmonyPatch(typeof(ItemSpawnerUI), "ButtonPress_DetailTile")]
        [HarmonyPostfix]
        private static void ButtonPressDetailPatch(int i, ItemSpawnerUI __instance)
        {
            if(__instance.m_IDSelectedForSpawn.Infographic != null)
            {
                __instance.ControlPoster.gameObject.SetActive(true);
                __instance.ControlPoster.material.SetTexture("_MainTex", __instance.m_IDSelectedForSpawn.Infographic.Poster);
            }
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


        public static int GetMaxCategoryPage(ItemSpawnerCategoryDefinitions.Category category)
        {
            int numCats = 0;

            foreach (ItemSpawnerCategoryDefinitions.SubCategory sub in category.Subcats)
            {
                bool displayCategory = sub.DoesDisplay_Sandbox;
                if (GM.CurrentSceneSettings.UsesUnlockSystem) displayCategory = sub.DoesDisplay_Unlocks;

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


        public static List<ItemSpawnerCategoryDefinitions.SubCategory> GetVisibleSubCategories(ItemSpawnerCategoryDefinitions.Category category)
        {
            List<ItemSpawnerCategoryDefinitions.SubCategory> visible = new List<ItemSpawnerCategoryDefinitions.SubCategory>();

            foreach (ItemSpawnerCategoryDefinitions.SubCategory sub in category.Subcats)
            {
                bool displayCategory = sub.DoesDisplay_Sandbox;
                if (GM.CurrentSceneSettings.UsesUnlockSystem) displayCategory = sub.DoesDisplay_Unlocks;

                if (displayCategory) visible.Add(sub);
            }

            return visible;
        }

    }


    public class ItemSpawnerDataObject : MonoBehaviour
    {
        public Text loadingText;

        public int currHomePage = 0;
        public int currCategoryPage = 0;

        public int maxHomePage = 0;
        public int maxCategoryPage = 0;


        private void Awake()
        {
            CreateLoadingText();

            loadingText.gameObject.SetActive(false);

            LoaderStatus.ProgressUpdated += UpdateText;
        }

        private void OnDestroy()
        {
            LoaderStatus.ProgressUpdated -= UpdateText;
        }

        private void UpdateText(float progress)
        {
            if(progress < 1)
            {
                loadingText.gameObject.SetActive(true);
                loadingText.text = "Loading Items : " + (int)(progress * 100) + "%";
            }

            else
            {
                loadingText.gameObject.SetActive(false);
            }
        }

        private void CreateLoadingText()
        {
            GameObject canvas = new GameObject("LoadingTextCanvas");
            canvas.transform.SetParent(transform);
            canvas.transform.rotation = transform.rotation;
            canvas.transform.localPosition = Vector3.zero;

            Canvas canvasComp = canvas.AddComponent<Canvas>();
            RectTransform rect = canvasComp.GetComponent<RectTransform>();
            canvasComp.renderMode = RenderMode.WorldSpace;
            rect.sizeDelta = new Vector2(1, 1);

            GameObject text = new GameObject("LoadingText");
            text.transform.SetParent(canvas.transform);
            text.transform.rotation = transform.parent.rotation;
            text.transform.localPosition = Vector3.zero + Vector3.up * 0.4f + Vector3.left * 0.25f;

            text.AddComponent<CanvasRenderer>();
            Text textComp = text.AddComponent<Text>();
            Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

            textComp.text = "EXAMPLE TEXT";
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.fontSize = 32;
            text.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);
            textComp.font = ArialFont;
            textComp.horizontalOverflow = HorizontalWrapMode.Overflow;

            loadingText = textComp;
        }


    }

}
