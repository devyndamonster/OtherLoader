using System;
using System.Collections.Generic;
using System.Linq;
using ADepIn;
using FistVR;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace OtherLoader
{
	public static class QuickbeltPanelPatch
	{
		public const int QBS_PER_PAGE = 14;
		//this entire thing just patches the quickbelt buttons to be dynamic and adds em in properly
		[HarmonyPatch(typeof(OptionsScreen_Quickbelt), "Awake")]
		[HarmonyPrefix]
		public static bool Patch_AddScreens(OptionsScreen_Quickbelt __instance)
		{
			//make the page handler to handle multiple pages
			var pageHandler = __instance.gameObject.AddComponent<QBslotPageHandler>();
			pageHandler.QBslotButtonSet = __instance.OBS_SlotStyle;
			
			GameObject template = __instance.OBS_Handedness.ButtonsInSet[0].gameObject; //get and save a template- we'll use this to make the other buttons
			foreach (FVRPointableButton qb in __instance.OBS_SlotStyle.ButtonsInSet) //delete all the currently existing screens
				Object.Destroy(qb.gameObject);

			//remake the ButtonsInSet array that holds all the QB buttons
			__instance.OBS_SlotStyle.ButtonsInSet = new FVRPointableButton[GM.Instance.QuickbeltConfigurations.Length];
			for (var i = 0; i < __instance.OBS_SlotStyle.ButtonsInSet.Length; i++)
			{
				int pagePos = i % QBS_PER_PAGE; //14 QBs per page + 2 for page nav (qbsperpage is default 14)
				int column = pagePos % 4; // 4 QBs per column
				var row = (int)Mathf.Floor((pagePos / 4f)); //4QBs per row

				//set fistvr button location / references
				//spawn off template; set proper parent
				OtherLogger.Log("Adding QB " + GM.Instance.QuickbeltConfigurations[i].name, OtherLogger.LogType.Loading);
				GameObject newButton = Object.Instantiate(template, __instance.OBS_SlotStyle.transform, true); 
				FVRPointableButton newButtonsButton = newButton.GetComponent<FVRPointableButton>(); //get pointablebutton
				__instance.OBS_SlotStyle.ButtonsInSet[i] = newButtonsButton; //add the pointable button to the list
				string QBname = GM.Instance.QuickbeltConfigurations[i].name.Split('_').Last(); //get name based off prefab name
				Button uiButton = SetQBSlotOptionsPanelButton(newButton, row, column, QBname);

				//Before you ask, "__instance.SetSlotStyle(i)" will not work
				//what it calls moves with i, and i can't get it to stay. neither will
				//some hack that makes a new int on the fly based off i. 
				
				//set the QB style
				uiButton.onClick.AddListener(() => { __instance.SetSlotStyle(uiButton.transform.GetSiblingIndex()); });
				//tell the button group handler the button's been pressed
				uiButton.onClick.AddListener(()  =>{ __instance.OBS_SlotStyle.SetSelectedButton(uiButton.transform.GetSiblingIndex()); });
			}
			
			//make last page button. see the forloop for comments, it's basically the same
			GameObject pageButton = Object.Instantiate(template, __instance.OBS_SlotStyle.transform, true);
			Button pageButtonUIButton = SetQBSlotOptionsPanelButton(pageButton, 3, 2, "Previous Page");
			pageButtonUIButton.onClick.AddListener(() => { pageHandler.GotoPreviousPage(); });
			pageHandler.ButtonPreviousPage = pageButtonUIButton.gameObject; //set pageHandler's prev page button
			
			//make next page button.
			pageButton = Object.Instantiate(template, __instance.OBS_SlotStyle.transform, true);
			pageButtonUIButton = SetQBSlotOptionsPanelButton(pageButton, 3, 3, "Next Page");
			pageButtonUIButton.onClick.AddListener(() => { pageHandler.GotoNextPage(); });
			pageHandler.ButtonNextPage = pageButtonUIButton.gameObject; //set pageHandler's next page button
			return true;
		}

		public static Button SetQBSlotOptionsPanelButton(GameObject button, int row, int column, string text)
		{
			//localposition multiplier to position the button
			float buttonX = -100 + (125 * column);
			float buttonY = -40 + (-45 * row);
			//set the localpositions
			button.transform.localPosition = new Vector3(buttonX, buttonY, button.transform.localPosition.z);
			//set the text
			button.gameObject.transform.GetChild(0).GetComponent<Text>().text = text;
			//remove listeners; return button for adding listeners
			var uiButton = button.GetComponent<Button>(); 
			uiButton.onClick.RemoveAllListeners();
			return uiButton;
		}

		[HarmonyPatch(typeof(OptionsScreen_Quickbelt), "InitScreen")]
		[HarmonyPrefix]
		public static bool Patch_OutOfIndexPreventer()
		{
			if (GM.Options.QuickbeltOptions.QuickbeltPreset > GM.Instance.QuickbeltConfigurations.Length)
			{
				GM.Options.QuickbeltOptions.QuickbeltPreset = 0;
			}
			return true;
		}
	}

	public class QBslotPageHandler : MonoBehaviour
	{
		public OptionsPanel_ButtonSet QBslotButtonSet;
		public GameObject ButtonNextPage;
		public GameObject ButtonPreviousPage;
		public int currentPage;

		public void Start()
		{
			SetVisibility();
		}

		public void SetButtons()
		{
			ButtonNextPage.SetActive(true);
			ButtonPreviousPage.SetActive(true);
			if (currentPage <= 0) ButtonPreviousPage.SetActive(false);
			int pages = Mathf.CeilToInt(QBslotButtonSet.ButtonsInSet.Length / QuickbeltPanelPatch.QBS_PER_PAGE);
			if(currentPage >= pages) ButtonNextPage.SetActive(false);
		}

		public void SetVisibility()
		{
			SetButtons();
			//disable every button
			foreach (FVRPointableButton button in QBslotButtonSet.ButtonsInSet) button.gameObject.SetActive(false);
			int startPoint = currentPage * QuickbeltPanelPatch.QBS_PER_PAGE; //the first button in the page
			int endPoint = startPoint + QuickbeltPanelPatch.QBS_PER_PAGE; //the end point
			
			//set every button from startpoint to endpoint to active
			for (int i = startPoint; i < endPoint; i++)
			{
				if (i >= QBslotButtonSet.ButtonsInSet.Length) return;
				QBslotButtonSet.ButtonsInSet[i].gameObject.SetActive(true);
			}
		}
		
		public void GotoPreviousPage()
		{
			currentPage--;
			SetVisibility();
		}

		public void GotoNextPage()
		{
			currentPage++;
			SetVisibility();
		}
	}
}
