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
	public class QuickbeltPanelPatch
	{
		public static readonly int QBsPerPage = 14;
		//this entire thing just patches the quickbelt buttons to be dynamic and adds em in properly
		[HarmonyPatch(typeof(OptionsScreen_Quickbelt), "Awake")]
		[HarmonyPrefix]
		public static bool Patch_AddScreens(OptionsScreen_Quickbelt __instance)
		{
			//make the page handler to handle multiple pages
			var pageHandler = __instance.gameObject.AddComponent<QBslotPageHandler>();
			pageHandler.QBslotButtonSet = __instance.OBS_SlotStyle;
			
			var template = __instance.OBS_Handedness.ButtonsInSet[0].gameObject; //get and save a template- we'll use this to make the other buttons
			foreach (var qb in __instance.OBS_SlotStyle.ButtonsInSet) //delete all the currently existing screens
				Object.Destroy(qb.gameObject);
			
			//add the extra pages and page switch button
			//TODO: do that please, but also do it later i hate this
			
			//remake the ButtonsInSet array that holds all the QB buttons
			__instance.OBS_SlotStyle.ButtonsInSet = new FVRPointableButton[GM.Instance.QuickbeltConfigurations.Length];
			for (int i = 0; i < __instance.OBS_SlotStyle.ButtonsInSet.Length; i++)
			{
				int pagePos = i % QBsPerPage; //14 QBs per page + 2 for page nav (qbsperpage is default 14)
				int column = pagePos % 4; // 4 QBs per column
				int row = (int) Mathf.Floor((pagePos / 4f)); //4QBs per row

				//set fistvr button location / references
				//spawn off template; set proper parent
				OtherLogger.Log("Adding QB " + GM.Instance.QuickbeltConfigurations[i].name, OtherLogger.LogType.Loading);
				GameObject newButton = Object.Instantiate(template, __instance.OBS_SlotStyle.transform, true); 
				FVRPointableButton newButtonsButton = newButton.GetComponent<FVRPointableButton>(); //get pointablebutton
				__instance.OBS_SlotStyle.ButtonsInSet[i] = newButtonsButton; //add the pointable button to the list
				string QBname = GM.Instance.QuickbeltConfigurations[i].name.Split('_').Last(); //get name based off prefab name
				Button UnityUIButton = SetQBSlotOptionsPanelButton(newButton, row, column, QBname);

				//Before you ask, "__instance.SetSlotStyle(i)" will not work
				//what it calls moves with i, and i can't get it to stay. neither will
				//some hack that makes a new int on the fly based off i. 
				
				//set the QB style
				UnityUIButton.onClick.AddListener(delegate { __instance.SetSlotStyle(UnityUIButton.transform.GetSiblingIndex()); });
				//tell the button group handler the button's been pressed
				UnityUIButton.onClick.AddListener(delegate { __instance.OBS_SlotStyle.SetSelectedButton(UnityUIButton.transform.GetSiblingIndex()); });
			}
			
			//make last page button. see the forloop for comments, it's basically the same
			GameObject pageButton = Object.Instantiate(template, __instance.OBS_SlotStyle.transform, true);
			Button pageButtonUIButton = SetQBSlotOptionsPanelButton(pageButton, 3, 2, "Previous Page");
			pageButtonUIButton.onClick.AddListener(delegate { pageHandler.GotoPreviousPage(); });
			pageHandler.ButtonPreviousPage = pageButtonUIButton.gameObject; //set pageHandler's prev page button
			
			//make next page button.
			pageButton = Object.Instantiate(template, __instance.OBS_SlotStyle.transform, true);
			pageButtonUIButton = SetQBSlotOptionsPanelButton(pageButton, 3, 3, "Next Page");
			pageButtonUIButton.onClick.AddListener(delegate { pageHandler.GotoNextPage(); });
			pageHandler.ButtonNextPage = pageButtonUIButton.gameObject; //set pageHandler's next page button
			return true;
		}

		public static Button SetQBSlotOptionsPanelButton(GameObject button, int row, int column, string text)
		{
			//localposition multiplier to position the button
			float buttonx = -100 + (125 * column);
			float buttony = -40 + (-45 * row);
			//set the localpositions
			button.transform.localPosition = new Vector3(buttonx, buttony, button.transform.localPosition.z);
			//set the text
			button.gameObject.transform.GetChild(0).GetComponent<Text>().text = text;
			//remove listeners; return button for adding listeners
			Button UIbutton = button.GetComponent<Button>(); 
			UIbutton.onClick.RemoveAllListeners();
			return UIbutton;
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
			int pages = Mathf.CeilToInt(QBslotButtonSet.ButtonsInSet.Length / QuickbeltPanelPatch.QBsPerPage);
			if(currentPage >= pages) ButtonNextPage.SetActive(false);
		}

		public void SetVisibility()
		{
			SetButtons();
			//disable every button
			foreach (var button in QBslotButtonSet.ButtonsInSet) button.gameObject.SetActive(false);
			int startPoint = currentPage * QuickbeltPanelPatch.QBsPerPage; //the first button in the page
			int endPoint = startPoint + QuickbeltPanelPatch.QBsPerPage; //the end point
			
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
