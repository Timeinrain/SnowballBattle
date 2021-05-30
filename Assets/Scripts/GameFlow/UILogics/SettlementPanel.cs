using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SettlementPanel : MonoBehaviour
{
	public GameObject victoryPanel;
	public GameObject defeatPanel;
	public GameObject audio;
	public GameObject scoreDetail;

	[Button]
	public void StartSettle(bool victory)
	{
		audio.SetActive(false);
		if (victory)
		{
			victoryPanel.SetActive(true);
			foreach(var subAnimator in victoryPanel.GetComponentsInChildren<Animator>())
			{
				subAnimator.SetTrigger("Flash");
			}
		}
		else
		{
			defeatPanel.SetActive(true);
			foreach (var subAnimator in defeatPanel.GetComponentsInChildren<Animator>())
			{
				subAnimator.SetTrigger("Flash");
			}
		}
	}

	public void ShowPlayerScore()
	{
		scoreDetail.SetActive(true);
		scoreDetail.GetComponent<ScoresPanel>().StartSettle();
	}

	[Button]
	public void ResetPanels()
	{
		victoryPanel.SetActive(false);
		defeatPanel.SetActive(false);
	}

	public void Return()
	{
		Application.Quit();
		victoryPanel.SetActive(false);
		defeatPanel.SetActive(false);
		//UIMgr._Instance.PanelSwitchFromTo(gameObject, UIMgr._Instance.inRoomUI, null, false);
	}
}
