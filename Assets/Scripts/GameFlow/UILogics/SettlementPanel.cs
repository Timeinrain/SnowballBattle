using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SettlementPanel : MonoBehaviour
{
	public GameObject victoryPanel;
	public GameObject defeatPanel;
	[Button]
	public void StartSettle(bool victory)
	{
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

	[Button]

	public void ResetPanels()
	{
		victoryPanel.SetActive(false);
		defeatPanel.SetActive(false);
	}

	public void Return()
	{
		victoryPanel.SetActive(false);
		defeatPanel.SetActive(false);
		UIMgr._Instance.PanelSwitchFromTo(gameObject, UIMgr._Instance.inRoomUI, null, false);
	}
}
