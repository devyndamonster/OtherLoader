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

		//this entire thing just patches the quickbelt buttons to be dynamic and adds em in properly
		[HarmonyPatch(typeof(OptionsScreen_Quickbelt), "Awake")]
		[HarmonyPrefix]
		public static bool Patch_AddScreens(OptionsScreen_Quickbelt __instance)
		{
			//get a template- we'll use this to make the other buttons
			var template = __instance.OBS_Handedness.ButtonsInSet[0].gameObject;
			//delete all the currently existing screens
			foreach (var qb in __instance.OBS_SlotStyle.ButtonsInSet)
				Object.Destroy(qb.gameObject);
			//add the extra pages and page switch button
			//TODO: do that please, but also do it later i hate this
			
			//add the new screens
			__instance.OBS_SlotStyle.ButtonsInSet = new FVRPointableButton[GM.Instance.QuickbeltConfigurations.Length];
			Debug.Log("There are currently " + GM.Instance.QuickbeltConfigurations.Length + " quickbelts detected!");
			for (int i = 0; i < __instance.OBS_SlotStyle.ButtonsInSet.Length; i++)
			{
				//TODO: is there a way to not have so many getcomponents???
				//get column num (0-3)
				int column = i % 4;
				//get row num
				int row = (int) Mathf.Floor((i / 4f));
				//calc x / y localpos
				float x = -100 + (125 * column);
				float y = -40 + (-45 * row);

				//set fistvr button location / references
				GameObject newButton =
					Object.Instantiate(template, __instance.OBS_SlotStyle.transform,
						true); //spawn off template; set proper parent
				FVRPointableButton
					newButtonsButton = newButton.GetComponent<FVRPointableButton>(); //get pointablebutton
				__instance.OBS_SlotStyle.ButtonsInSet[i] = newButtonsButton; //add the pointable button to the list
				Vector3 lp = newButton.transform.localPosition;
				lp.x = x;
				lp.y = y;
				newButton.transform.localPosition = lp;

				//get text
				Text buttonText = newButton.gameObject.transform.GetChild(0).GetComponent<Text>(); //get text
				buttonText.text =
					GM.Instance.QuickbeltConfigurations[i].name.Split('_').Last(); //get name based off prefab name
				Debug.Log("Adding QB slot " + GM.Instance.QuickbeltConfigurations[i].name);

				//set button
				Button UnityUIButton = newButton.GetComponent<Button>(); //get button
				UnityUIButton.onClick.RemoveAllListeners(); //remove all button invokes
				//okokok having to get the sibling index to check what number the button corresponds to
				//looks and sounds very stupid, but if you insert i in there it just breaks for some reason
				//well not for "some reason", it's because it fucking *follows* i
				//so all of the buttons call i for sOME REASON
				//fuck you unity i hate you
				UnityUIButton.onClick.AddListener(delegate
				{
					__instance.SetSlotStyle(UnityUIButton.transform.GetSiblingIndex());
				});
				UnityUIButton.onClick.AddListener(delegate
				{
					__instance.OBS_SlotStyle.SetSelectedButton(UnityUIButton.transform.GetSiblingIndex());
				});
			}

			return true;
		}

		[HarmonyPatch(typeof(OptionsScreen_Quickbelt), "InitScreen")]
		[HarmonyPrefix]
		public static bool Patch_c()
		{
			if (GM.Options.QuickbeltOptions.QuickbeltPreset > GM.Instance.QuickbeltConfigurations.Length)
			{
				GM.Options.QuickbeltOptions.QuickbeltPreset = 0;
			}
			return true;
		}
	}
}
