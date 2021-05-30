using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class SettingsPanel : PanelBase
{

	public Slider BGMSlider;
	public Slider musicFXSlide;
	public Button ChineseLang;
	public Button EngLang;
    public void SetLastPanel(GameObject panel)
	{
		lastPanel = panel;
	}

	public void CheckMochiActive()
	{
		if (lastPanel == UIMgr._Instance.inRoomUI)
		{
			UIMgr._Instance.inRoomUI.GetComponent<InRoom>().MochiInactive(true);
		}
	}

	public override void Return()
	{
		if (lastPanel == null) {
			gameObject.SetActive(false);
		}
		base.Return();
	}

	
}
