using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
/// <summary>
/// 网络初始化脚本
/// 加载场景、场景切换
/// 
/// 异步切换场景
/// </summary>
public class NetworkInit : MonoBehaviourPunCallbacks
{
	[Header("控制面板展示顺序")]
	public GameObject loginUI;
	public GameObject lobbyListUI;
	public GameObject roomListUI;
	public GameObject createRoomUI;
	public GameObject loadingPanel;

	[Header("读取的要同步的本地玩家信息")]
	public Text lobbyName;
	public Text userName;
	public Text roomName;

	[Header("同一个房间的最大玩家数目")]
	public byte maxPlayers = 4;

	[Header("房间信息")]
	public Transform roomContainer;
	public GameObject roomPrefab;


	/// <summary>
	/// For Async load scene.
	/// </summary>
	private void Start()
	{
		DontDestroyOnLoad(gameObject);
		PhotonNetwork.AutomaticallySyncScene = true;
		PhotonNetwork.SendRate = 90;
	}

	/// <summary>
	/// Controlling the Return Button Logic
	/// </summary>
	#region Return Operations

	public void ReturnToLogInPanel()
	{
		roomListUI.SetActive(false);
		loginUI.SetActive(true);
		PhotonNetwork.Disconnect();
	}

	public void ReturnToRoomSelectingPanel()
	{
		createRoomUI.SetActive(false);
	}

	#endregion

	/// <summary>
	/// Responce when Log In button is clicked.
	/// </summary>
	public void LogIn()
	{
		loadingPanel.SetActive(true);
		ConnectToChina();
		PhotonNetwork.SendRate = 60;
		PhotonNetwork.LocalPlayer.NickName = userName.text;
		print("Connect to China region.");
	}

	/// <summary>
	/// Presettings
	/// </summary>
	void ConnectToChina()
	{
		PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "cn";
		PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
		PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = "173ad590-47f0-4573-b1cb-3be35654688b";
		PhotonNetwork.PhotonServerSettings.AppSettings.Server = "ns.photonengine.cn";
		PhotonNetwork.ConnectUsingSettings();
	}

	/// <summary>
	/// Auto call when connected to master
	/// </summary>
	public override void OnConnectedToMaster()
	{
		loadingPanel.SetActive(false);
		loginUI.SetActive(false);
		//lobby disabled
		//lobbyListUI.SetActive(true);
		roomListUI.SetActive(true);
		base.OnConnectedToMaster();
		Debug.Log("Connected!");
	}

	/// <summary>
	/// Call when "创建房间" clicked
	/// </summary>
	public void StartCreatingRoom()
	{
		createRoomUI.SetActive(true);
	}

	/// <summary>
	/// Create and Enter the room
	/// </summary>
	public void CreateRoom(string roomName, byte maxPlayer)
	{
		loadingPanel.SetActive(true);
		PhotonNetwork.CreateRoom(roomName, new RoomOptions()
		{
			MaxPlayers = maxPlayer,
		},
		default);
	}

	/// <summary>
	/// Join room button clicked
	/// </summary>
	/// <param name="roomID"></param>
	public void JoinRoom(string roomName)
	{
		loadingPanel.SetActive(true);
		PhotonNetwork.JoinRoom(roomName);
	}

	/// <summary>
	/// Join Random Room
	/// </summary>
	public void JoinRandomRoom()
	{
		loadingPanel.SetActive(true);
		PhotonNetwork.JoinRandomRoom();
	}

	/// <summary>
	/// Why can't join room.
	/// </summary>
	/// <param name="returnCode"></param>
	/// <param name="message"></param>
	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		loadingPanel.SetActive(false);
		base.OnJoinRandomFailed(returnCode, message);
		Debug.Log(message);
		createRoomUI.SetActive(true);
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
		loadingPanel.SetActive(false);
		base.OnJoinedRoom();
		Debug.Log("Joined Room!");
		//LoadingUI DontDestroyOnLoad
		//PhotonNetwork.LoadLevel(1);
		//PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-1, 1), 3, Random.Range(-1, 1)), Quaternion.identity, 0);
		//Load the room scene
		if (PhotonNetwork.IsMasterClient)
		{
			//todo  :   upload local data for sync
		}
		else
		{
			//todo  :   sync data
		}
	}


	public override void OnCreatedRoom()
	{
		base.OnCreatedRoom();
	}


	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		base.OnRoomListUpdate(roomList);
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
}