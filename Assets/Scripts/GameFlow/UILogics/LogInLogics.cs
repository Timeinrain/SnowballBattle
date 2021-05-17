using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using Photon.Pun;

public class LogInLogics : PanelBase
{
	public GameObject settingsPanel;

	[Header("读取的要同步的本地玩家信息")]
	public InputField account;
	public InputField password;
	public Animator accAnim;
	public Animator pwdAnim;

	private void OnEnable()
	{
		PhotonNetwork.Disconnect();
		password.text = "";
		accAnim.SetTrigger("FlyInRight");
		pwdAnim.SetTrigger("FlyInLeft");
	}

	public void LogIn()
	{
		if (NetWorkMgr._Instance.LogIn(account.text, password.text))
		{
			FindObjectOfType<UIMgr>().LogIn();
			print("Logged in!");
		}
		else throw new System.Exception("Can't not connect to the Internet.");
	}

	public void Register()
	{
		print("Not Available yet!.");
	}

	public override void CallSettings()
	{
		base.CallSettings();
		settingsPanel.SetActive(true);
		settingsPanel.GetComponent<SettingsPanel>().SetLastPanel(gameObject);
	}
}
