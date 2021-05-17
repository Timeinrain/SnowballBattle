using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using Sirenix.OdinInspector;

/// <summary>
/// 网络初始化脚本
/// 加载场景、场景切换
/// 异步切换场景
/// 
/// =======================READ ME==============================
/// 关于加载场景：
/// 1.使用 PanelSwitchFromTo 方法
/// 根据是否需要加载场景调整第三个参数。
/// 如果不需要loading界面，如简单的调用settings ui，则
/// 直接使用：
/// ===============================public void PanelSwitchFromTo(GameObject from, GameObject to, bool needLoading = false)
/// 函数即可。
/// 2.若需要loading界面，如使用网络加载，需要等待同步。则需要判定何时结束loading，因此需要在相应可以结束loading的时候发送一个退出loading动画的信号。
/// 使用方法：
/// ===============================private void AllowNeedLoadingTransitionEnter() 
/// 来发送信号，
/// 当协程监听到信号之后便会停止loading动画的播放。
/// ============================================================
/// </summary>
public class NetWorkMgr : MonoBehaviourPunCallbacks
{
	public static NetWorkMgr _Instance;
	public UIMgr _UIMgr;

	#region 参数面板

	[ReadOnly]
	[Header("本地客户端玩家信息，不可修改，自动生成")]
	public ClientInfo client;

	[Header("同一个房间的最大玩家数目")]
	public byte maxPlayers = 4;

	[Header("房间信息")]
	public Transform roomContainer;
	public GameObject roomPrefab;

	private List<RoomInfo> roomInfosCache;

	#endregion

	/// <summary>
	/// For Async load scene.
	/// </summary>
	private void Start()
	{
		DontDestroyOnLoad(gameObject);
		_UIMgr = FindObjectOfType<UIMgr>();
		_Instance = this;
		PhotonNetwork.AutomaticallySyncScene = true;
		PhotonNetwork.SendRate = 90;
	}

	/// <summary>
	/// Controlling the Return Button Logic
	/// </summary>
	#region Return Operations

	public void ReturnToLogInPanel()
	{
		_UIMgr.ReturnToLogInPanel();
		PhotonNetwork.Disconnect();
	}

	#endregion

	/// <summary>
	/// Responce when Log In button is clicked.
	/// </summary>
	public bool LogIn(string account, string passwd)
	{
		if (!ConnectToChina()) return false;
		PhotonNetwork.SendRate = 60;
		PhotonNetwork.LocalPlayer.NickName = account;
		client = new ClientInfo();
		client.SetInfo(account, passwd);
		print("Connect to China region.");
		return true;
	}

	/// <summary>
	/// Presettings
	/// </summary>
	bool ConnectToChina()
	{
		PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "cn";
		PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
		PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = "173ad590-47f0-4573-b1cb-3be35654688b";
		PhotonNetwork.PhotonServerSettings.AppSettings.Server = "ns.photonengine.cn";
		if (PhotonNetwork.ConnectUsingSettings())
		{
			return true;
		}
		else return false;
	}

	/// <summary>
	/// Auto call when connected to master
	/// </summary>
	public override void OnConnectedToMaster()
	{
		_UIMgr.AllowNeedLoadingTransitionEnter();
		base.OnConnectedToMaster();
		Debug.Log("Connected!");
	}


	/// <summary>
	/// Using Offline Mode
	/// </summary>
	public void StartOfflineMode()
	{
		//todo: Jump To Offline Panels

	}

	/// <summary>
	/// Create and Enter the room
	/// </summary>
	public void CreateRoom(string roomName, byte maxPlayer, GameMode gameMode)
	{
		_UIMgr.CreateRoom();
		RoomOptions roomOp = new RoomOptions()
		{
			MaxPlayers = maxPlayer,
			CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
			{
				{ gameMode, "GameMode" },
				//todo: MAP 改一下
				{ "MAP","Map"}
			},
			CustomRoomPropertiesForLobby = new string[] { "GameMode", "Map" },
			IsOpen = true,
			IsVisible = true,
		};
		PhotonNetwork.CreateRoom(roomName, roomOp, default);
	}

	/// <summary>
	/// Join room button clicked
	/// </summary>
	/// <param name="roomID"></param>
	public void JoinRoom(string roomName)
	{
		_UIMgr.JoinRoom();
		PhotonNetwork.JoinRoom(roomName);
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		base.OnJoinRoomFailed(returnCode, message);
		_UIMgr.OnJoinRoomFailed(message);
	}

	/// <summary>
	/// Join Random Room
	/// </summary>
	public void JoinRandomRoom()
	{
		_UIMgr.JoinRandomRoom();
		PhotonNetwork.JoinRandomRoom();
	}

	/// <summary>
	/// Why can't join room.
	/// </summary>
	/// <param name="returnCode"></param>
	/// <param name="message"></param>
	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		_UIMgr.OnJoinRandomFailed(message);
		base.OnJoinRandomFailed(returnCode, message);
		Debug.Log(message);
	}

	/// <summary>
	/// What causes the disconnection
	/// </summary>
	/// <param name="cause"></param>
	public override void OnDisconnected(DisconnectCause cause)
	{
		base.OnDisconnected(cause);
		switch (cause)
		{
			case DisconnectCause.DisconnectByClientLogic:
				{
					Debug.Log("User Logged Out!");
					break;
				}
			default:
				{
					Debug.Log("Unknown Error!");
					break;
				}
		}
	}

	/// <summary>
	/// Load the Room Scene
	/// </summary>
	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();
		Debug.Log("Joined Room!");
		//todo: 更新局外room 信息
		_UIMgr.AllowNeedLoadingTransitionEnter();

	}

	public override void OnCreatedRoom()
	{
		base.OnCreatedRoom();
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		base.OnRoomListUpdate(roomList);
		roomInfosCache = roomList;
		for (int i = 0; i < roomContainer.childCount; i++)
		{
			if (roomContainer.GetChild(i).gameObject.GetComponentInChildren<Text>().text == roomList[i].Name)
			{
				Destroy(roomContainer.GetChild(i).gameObject);

				if (roomList[i].PlayerCount == 0)
				{
					roomList.Remove(roomList[i]);
				}
			}
		}
		foreach (var room in roomList)
		{
			GameObject newRoom = Instantiate(roomPrefab, roomContainer.position, Quaternion.identity);
			//todo : 不知道怎么刷新房间所选地图
			newRoom.GetComponent<RoomConfig>().UpdateRoomInfo(room);
			newRoom.transform.SetParent(roomContainer);
		}
	}

	/// <summary>
	/// Get the room info list. 
	/// </summary>
	/// <returns>Type of List<RoomInfo>  </returns>
	public List<RoomInfo> GetRoomInfos()
	{
		return roomInfosCache;
	}
}

public enum TransitionType
{
	OnEnter = 0,
	OnPlay = 1,
	OnExit = 2
}