using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance = null;
    public Transform[] redTeamSpawnPoints;
    public Transform[] blueTeamSpawnPoints;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    /// <summary>
    /// ÿ���ͻ��˵�����������õ��ĳ����㲻һ�������ڶ���˳��
    /// </summary>
    /// <returns></returns>
    public Transform GetSpawnPoint(Player player)
    {
        //todo:ɾ������(���ز���)
        //return blueTeamSpawnPoints[0];

        List<Player> players = InOutGameRoomInfo.Instance.inRoomPlayerInfos;

        // ����һ�����ڶ���˳�������
        int index = 0;
        foreach(var pl in players)
        {
            if(player.team == pl.team)
            {
                if (player == pl) break;
                index++;
            }
        }

        if (player.team == Team.Red)
        {
            return redTeamSpawnPoints[index];
        }
        else
        {
            return blueTeamSpawnPoints[index];
        }
    }

    private void OnDrawGizmos()
    {
        foreach(var t in redTeamSpawnPoints)
        {
            Gizmos.DrawIcon(t.position, "spawn_point.png");
        }
        foreach (var t in blueTeamSpawnPoints)
        {
            Gizmos.DrawIcon(t.position, "spawn_point.png");
        }
    }
}
