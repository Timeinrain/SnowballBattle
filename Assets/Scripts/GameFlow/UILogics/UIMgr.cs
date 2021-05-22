using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMgr : MonoBehaviour
{
	public static UIMgr _Instance;
	[Header("控制面板展示顺序")]
	public GameObject loginUI;
	public GameObject roomListUI;
	public GameObject inRoomUI;
	public GameObject createRoomUI;
	public GameObject settingsUI;
	public GameObject mapSelectionUI;
	public GameObject loadingPanel;
	public GameObject switchMaskMgr;

	private SwitchMaskFX switchMaskFX;

	void Start()
	{
		_Instance = this;
		switchMaskFX = switchMaskMgr.GetComponent<SwitchMaskFX>();
	}

	public void ReturnToLogInPanel()
	{
		roomListUI.SetActive(false);
		loginUI.SetActive(true);
	}

	public void ReturnToRoomSelectingPanel()
	{
		createRoomUI.SetActive(false);
	}

	public void LogIn()
	{
		PanelSwitchFromTo(loginUI, roomListUI, null, true);
	}

	/// <summary>
	/// Call when "创建房间" clicked
	/// </summary>
	public void StartCreatingRoom()
	{
		createRoomUI.SetActive(true);
	}

	/// <summary>
	/// Join room button clicked
	/// </summary>
	public void JoinRoom()
	{
		PanelSwitchFromTo(roomListUI, inRoomUI, default, true);
	}

	/// <summary>
	/// Join Random Room
	/// </summary>
	public void JoinRandomRoom()
	{
		PanelSwitchFromTo(roomListUI, inRoomUI, default, true);
	}

	public void StartSelectingMap()
	{
		mapSelectionUI.SetActive(true);
	}

	/// <summary>
	/// Why can't join random room.
	/// </summary>
	public void OnJoinRandomFailed(string message)
	{
		WithdrawTransition();
		AllowNeedLoadingTransitionEnter();
		createRoomUI.SetActive(true);
		print("No Available Random Room Now!" + message);
		//todo : do something
	}

	/// <summary>
	/// Do sth
	/// </summary>
	public void OnJoinRoomFailed(string message)
	{
		WithdrawTransition();
		AllowNeedLoadingTransitionEnter();
		createRoomUI.SetActive(true);
		print("Join Room Failed!" + message);
	}

	#region Transition Animations 1.PanelSwitchFromTo 2.AllowNeedLoadingTransitionEnter

	/// <summary>
	/// Switch panel from a to b
	/// select whether need loading
	/// </summary>
	/// <param name="src">switch source panel</param>
	/// <param name="des">switch destination panel</param>
	/// <param name="needLoading">true if the loading process needs loading animation</param>
	public void PanelSwitchFromTo(GameObject src, GameObject des, GameObject button = null, bool needLoading = false)
	{
		if (needLoading)
		{
			AnimateTransition(TransitionType.OnExit, src, new Vector3(1920 / 2, 1080 / 2, 0));
			StartCoroutine(WaitForTransitionEnterMsg(des, src));
		}
		else
		{
			AnimateTransition(TransitionType.OnExit, src, new Vector3(1920 / 2, 1080 / 2, 0));
			AnimateTransition(TransitionType.OnEnter, des, new Vector3(1920 / 2, 1080 / 2, 0));
		}
	}

	/// <summary>
	/// send the msg of allowance of play next panel 
	/// </summary>
	public void AllowNeedLoadingTransitionEnter(bool allow = true)
	{
		allowTransitionEnter = allow;
	}
	public void WithdrawTransition()
	{
		withdrawTransition = true;
	}

	private bool allowTransitionEnter = false;

	private bool withdrawTransition = false;

	/// <summary>
	/// Listener for allowance of target panel's entrance
	/// </summary>
	/// <param name="targetPanel"></param>
	/// <returns></returns>
	IEnumerator WaitForTransitionEnterMsg(GameObject targetPanel, GameObject srcPanel)
	{
		while (true)
		{
			if (allowTransitionEnter)
			{
				break;
			}
			yield return new WaitForEndOfFrame();
		}
		allowTransitionEnter = false;
		if (withdrawTransition)
		{
			withdrawTransition = false;
			yield return new WaitForSeconds(0.5f);
			AnimateTransition(TransitionType.OnEnter, srcPanel, new Vector3(1920 / 2, 1080 / 2, 0));
		}
		else
		{
			AnimateTransition(TransitionType.OnEnter, targetPanel, new Vector3(1920 / 2, 1080 / 2, 0));
		}
	}

	/// <summary>
	/// AnimationFX body
	/// </summary>
	/// <param name="type"></param>
	private void AnimateTransition(TransitionType type, GameObject panel, Vector3 pos, bool needLoading = false)
	{
		switch (type)
		{
			case TransitionType.OnEnter:
				{
					switchMaskFX.PlayZoomOut(pos, panel, needLoading);
					break;
				}
			case TransitionType.OnExit:
				{
					switchMaskFX.PlayZoomIn(pos, panel, needLoading);
					break;
				}
			case TransitionType.OnPlay:
				{

					break;
				}
		}
	}
	#endregion

}
