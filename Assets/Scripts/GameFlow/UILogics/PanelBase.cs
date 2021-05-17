using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

/// <summary>
/// Base Class for UIPanels
/// </summary>
public abstract class PanelBase : MonoBehaviour
{
	public GameObject lastPanel;
	public GameObject currentPanel;
	public delegate void PanelEvents();
	public PanelEvents panelEvents;

	/// <summary>
	/// Return to last panel
	/// </summary>
	/// <param name="lastPanel"></param>
	public virtual void Return()
	{
		if (panelEvents != null)
			panelEvents();
		currentPanel.SetActive(false);
		lastPanel.SetActive(true);
	}

	/// <summary>
	/// Call the Settings Panel
	/// </summary>
	public virtual void CallSettings() { }
}
