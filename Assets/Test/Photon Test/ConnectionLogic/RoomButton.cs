using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using PhotonTest;

namespace PhotonTest
{
	public class RoomButton : MonoBehaviourPunCallbacks
	{
		// Start is called before the first frame update
		Button roomButton;
		NetworkInit networkMgr;
		private void Start()
		{
			roomButton = GetComponent<Button>();
			networkMgr = FindObjectOfType<NetworkInit>();
		}

		public void OnClick()
		{
			networkMgr.JoinRoomButtonOnClick(roomButton.GetComponentInChildren<Text>().text);
		}
	}

}
