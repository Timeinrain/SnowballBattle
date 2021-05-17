using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// To collect the local client's information
/// </summary>
public class ClientInfo
{
	[ReadOnly] public string userName;
	[ReadOnly] public string password;
	[ReadOnly] public string roomId;
	[ReadOnly] public Team team;

	/// <summary>
	/// Initialize a local account information
	/// </summary>
	/// <param name="uName"></param>
	/// <param name="uPwd"></param>
	public void SetInfo(string uName, string uPwd, string rId = "", Team t = Team.Blue)
	{
		userName = uName;
		password = uPwd;
		roomId = rId;
		team = t;
	}
}
