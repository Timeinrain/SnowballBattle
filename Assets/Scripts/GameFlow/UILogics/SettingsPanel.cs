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

	public override void Return()
	{
		if (lastPanel == null) {
			throw new System.Exception("No lastPanel.");
		}
		base.Return();
	}

	
}
