using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
namespace PhotonTest.MultiCameraTest
{
    public class MultiCameraTest : MonoBehaviourPunCallbacks
    {
        public List<GameObject> playerInfos;
        void Start()
        {
            ConnectToChina();
            PhotonNetwork.SendRate = 90;
        }

        void ConnectToChina()
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "cn";
            PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = "173ad590-47f0-4573-b1cb-3be35654688b";
            PhotonNetwork.PhotonServerSettings.AppSettings.Server = "ns.photonengine.cn";
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            Debug.Log("Connected!");
            PhotonNetwork.JoinLobby();

        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            Debug.Log("Joined Lobby!");
            PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions() { MaxPlayers = 4 }, default);
        }

        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();
            if (playerInfos == null)
            {
                playerInfos = new List<GameObject>() { };
            }

        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            Debug.Log("Joined Room!");
            Team teamName = Team.Blue;
            GameObject go = PhotonNetwork.Instantiate("Mochi", new Vector3(Random.Range(-1, 1), 3, Random.Range(-1, 1)), Quaternion.identity, 0);
            go.tag = teamName.ToString() + "Team";
            go.GetComponent<Character>().id = Random.value.ToString();
            //Join team.
            GameObject teamObj = GameObject.FindWithTag(teamName.ToString() + "Team");
            go.transform.SetParent(teamObj.transform);
            playerInfos.Add(go);
            //photonView.RPC("GetNewPlayerInfo", RpcTarget.All, go.GetComponent<Character>().id);
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            GetNewPlayerInfo();
        }

        [PunRPC]
        public void GetNewPlayerInfo()
        {
            Character[] temp = FindObjectsOfType<Character>();
            playerInfos.Clear();
            foreach(var i in temp)
            {
                playerInfos.Add(i.gameObject);
            }

        }

        [PunRPC]
        public void SyncPlayersInfo()
        {

        }
    }
}